using Booking.API.Models;
using FinalProject.NET.DBcontext;
using FinalProject.NET.Dtos;
using FinalProject.NET.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace FinalProject.NET.Controllers
{
    [Route("api/auth/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<Person> _userManager;
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IDistributedCache _cache;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<Person> userManager,
            AppDbContext context,
            IEmailService emailService,
            IDistributedCache cache,
            IWebHostEnvironment env,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _context = context;
            _emailService = emailService;
            _cache = cache;
            _env = env;
            _logger = logger;
        }
        [HttpGet("specializations")]
        public async Task<IActionResult> GetSpecializations()
        {
            var data = await _context.Specializations
                .Select(s => new
                {
                    id = s.Id,   // Guid
                    name = s.Name
                })
                .ToListAsync();

            return Ok(data);
        }


        [HttpPost("register-user")]
        public async Task<IActionResult> RegisterUser([FromForm] RegisterUserDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);



            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Email,
                AccountStatus = AccountStatus.Active
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded) return BadRequest(createResult.Errors.Select(e => e.Description));

            // ----------- Generate Email Confirmation Token ------------
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // لازم نعمل encode لأن ال token بيبقى فيه رموز
            var encodedToken = Uri.EscapeDataString(token);

            // اللينك إلي هستخدمه في ال frontend
            var confirmationLink = $"{Request.Scheme}://{Request.Host}/api/auth/confirm-email?userId={user.Id}&token={encodedToken}";
            var path = Path.Combine(_env.ContentRootPath, "EmailTemplates", "EmailConfirmation.html");
            var html = await System.IO.File.ReadAllTextAsync(path);

            html = html.Replace("{{CONFIRMATION_LINK}}", confirmationLink);
            // ابعت الإيميل (إنت عندك service جاهزة أو هتكتب واحدة)
            await _emailService.SendEmailAsync(model.Email,
                "تأكيد الحساب",
                html);

            return StatusCode(StatusCodes.Status201Created,
                new { success = true, message = "تم إنشاء الحساب! برجاء فحص بريدك الإلكتروني لتفعيل الحساب." });
        }
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest("Invalid email confirmation request.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found.");

            var decodedToken = Uri.UnescapeDataString(token);

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (result.Succeeded)
                return Ok(new { success = true, message = "تم تفعيل الحساب بنجاح!" });

            return BadRequest(result.Errors);
        }


        [HttpPost("register-lawyer")]
        public async Task<IActionResult> RegisterLawyer([FromForm] RegisterLawyerDto dto)
        {
            if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
                return BadRequest("Email and Password are required");

            var lawyer = new Lawyer
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                About = dto.About,
                YearsOfExperience = dto.YearsOfExperience,
                OfficeLocation = dto.Location != null ? new Location
                {
                    Country = dto.Location.Country,
                    Government = dto.Location.Government,
                    City = dto.Location.City,
                    Street = dto.Location.Street
                } : null
            };

            // إنشاء المستخدم داخل Identity
            var result = await _userManager.CreateAsync(lawyer, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // إضافة التخصصات
            if (dto.Specializations != null && dto.Specializations.Any())
            {
                lawyer.LawyerSpecializations = dto.Specializations
                    .Select(spec => new LawyerSpecialization
                    {
                        LawyerId = lawyer.Id,
                        SpecializationId = spec
                    })
                    .ToList();
            }

            // رفع الوثائق
            await HandleDocuments(lawyer, dto);

            // حفظ كل التغييرات
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Lawyer registered successfully",
                LawyerId = lawyer.Id
            });
        }

        private async Task HandleDocuments(Lawyer lawyer, RegisterLawyerDto dto)
        {
            if (lawyer.Documents == null)
                lawyer.Documents = new List<DocumentVerification>();

            var files = new Dictionary<DocumentType, IFormFile>
                {
                    { DocumentType.IdFront, dto.IdFront },
                    { DocumentType.IdBack, dto.IdBack },
                    { DocumentType.SelfieWithId, dto.SelfieWithId },
                    { DocumentType.LicensePhoto, dto.LicensePhoto }
                };

            foreach (var kv in files)
            {
                var file = kv.Value;
                if (file != null && file.Length > 0)
                {
                    var folderPath = Path.Combine(_env.WebRootPath, "uploads", "lawyers", lawyer.Id.ToString());
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // URL مناسب للواجهة
                    var url = $"/uploads/lawyers/{lawyer.Id}/{fileName}";

                    lawyer.Documents.Add(new DocumentVerification
                    {
                        Id = Guid.NewGuid(),
                        LawyerId = lawyer.Id,
                        DocumentType = kv.Key,
                        FileUrl = url,
                        Status = VerificationStatus.Pending,
                        UploadedAt = DateTime.UtcNow
                    });
                }
            }
        }

        // مثال على دالة لتشفير الباسورد
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }








    }
}
