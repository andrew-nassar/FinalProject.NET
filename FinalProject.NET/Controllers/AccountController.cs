using Booking.API.Models;
using FinalProject.NET.DBcontext;
using FinalProject.NET.Dtos;
using FinalProject.NET.Models;
using FinalProject.NET.Services.Cloudinary;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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
        private readonly ICloudService _cloudService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<Person> userManager,
            AppDbContext context,
            IEmailService emailService,
            IDistributedCache cache,
            IWebHostEnvironment env,
            ICloudService cloudService,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _context = context;
            _emailService = emailService;
            _cache = cache;
            _env = env;
            _cloudService = cloudService;
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
            var validation = ValidateInput(dto);
            if (validation != null)
                return BadRequest(validation);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var location = await CreateLocationAsync(dto);

                var lawyer = await CreateLawyerUserAsync(dto, location.Id);
                if (lawyer == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest("Failed to create user.");
                }

                await AssignSpecializationsAsync(dto, lawyer.Id);

                var uploadedUrls = await HandleDocumentUploadsAsync(dto, lawyer.Id);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await SendConfirmationEmailAsync(dto.Email, lawyer.Id);

                return Ok(new
                {
                    Message = "Lawyer registered successfully. Verification email sent.",
                    LawyerId = lawyer.Id
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "RegisterLawyer failed");

                return StatusCode(500, "An error occurred while registering the lawyer");
            }
        }

        private string ValidateInput(RegisterLawyerDto dto)
        {
            if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
                return "Email and Password are required";

            if (dto.Location == null)
                return "Location is required";

            return null;
        }

        private async Task<Location> CreateLocationAsync(RegisterLawyerDto dto)
        {
            var location = new Location
            {
                Country = dto.Location.Country,
                Government = dto.Location.Government,
                City = dto.Location.City,
                Street = dto.Location.Street
            };

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return location;
        }
        private async Task<Lawyer> CreateLawyerUserAsync(RegisterLawyerDto dto, Guid locationId)
        {
            var lawyer = new Lawyer
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                UserName = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                About = dto.About,
                YearsOfExperience = dto.YearsOfExperience,
                OfficeLocationId = locationId
            };

            var result = await _userManager.CreateAsync(lawyer, dto.Password);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to create user: {Errors}", result.Errors);
                return null;
            }

            // مهم: نفصل التراكينج
            _context.Entry(lawyer).State = EntityState.Detached;

            return await _context.Lawyers
                .FirstOrDefaultAsync(l => l.Id == lawyer.Id);
        }

        private async Task AssignSpecializationsAsync(RegisterLawyerDto dto, Guid lawyerId)
        {
            if (dto.Specializations == null || !dto.Specializations.Any())
                return;

            var requested = dto.Specializations.Distinct().ToList();

            var validIds = await _context.Specializations
                .Where(s => requested.Contains(s.Id))
                .Select(s => s.Id)
                .ToListAsync();

            var existing = await _context.LawyerSpecializations
                .Where(ls => ls.LawyerId == lawyerId)
                .Select(ls => ls.SpecializationId)
                .ToListAsync();

            var toAdd = validIds.Except(existing).ToList();

            if (toAdd.Any())
            {
                var items = toAdd.Select(id => new LawyerSpecialization
                {
                    LawyerId = lawyerId,
                    SpecializationId = id
                });

                _context.LawyerSpecializations.AddRange(items);
            }
        }

        private async Task<List<string>> HandleDocumentUploadsAsync(RegisterLawyerDto dto, Guid lawyerId)
        {
            var uploadedUrls = new List<string>();

            var files = new Dictionary<DocumentType, IFormFile>
    {
        { DocumentType.IdFront, dto.IdFront },
        { DocumentType.IdBack, dto.IdBack },
        { DocumentType.SelfieWithId, dto.SelfieWithId },
        { DocumentType.LicensePhoto, dto.LicensePhoto }
    };

            foreach (var x in files)
            {
                if (x.Value == null || x.Value.Length == 0)
                    continue;

                var fileUrl = await _cloudService.UploadAsync(x.Value);
                if (string.IsNullOrEmpty(fileUrl))
                    continue;

                uploadedUrls.Add(fileUrl);

                _context.DocumentVerifications.Add(new DocumentVerification
                {
                    Id = Guid.NewGuid(),
                    LawyerId = lawyerId,
                    DocumentType = x.Key,
                    FileUrl = fileUrl,
                    Status = VerificationStatus.Pending,
                    UploadedAt = DateTime.UtcNow,
                    Notes = ""
                });
            }

            return uploadedUrls;
        }

        private async Task SendConfirmationEmailAsync(string email, Guid lawyerId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(lawyerId.ToString());
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var encoded = Uri.EscapeDataString(token);

                var link = $"{Request.Scheme}://{Request.Host}/api/auth/confirm-email?userId={lawyerId}&token={encoded}";

                var path = Path.Combine(_env.ContentRootPath, "EmailTemplates", "EmailConfirmation.html");
                var html = await System.IO.File.ReadAllTextAsync(path);

                html = html.Replace("{{CONFIRMATION_LINK}}", link);

                await _emailService.SendEmailAsync(email, "تأكيد الحساب", html);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email");
            }
        }

        private async Task HandleDocumentsCloud(Lawyer lawyer, RegisterLawyerDto dto)
        {
            if (lawyer.Documents == null)
                lawyer.Documents = new List<DocumentVerification>();

            // الملفات اللي هترفع
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
                    // رفع الملف على Cloudinary
                    var uploadedUrl = await _cloudService.UploadAsync(file);

                    // حفظ الـ URL في الـ DB
                    lawyer.Documents.Add(new DocumentVerification
                    {
                        Id = Guid.NewGuid(),
                        LawyerId = lawyer.Id,
                        DocumentType = kv.Key,
                        FileUrl = uploadedUrl,
                        Status = VerificationStatus.Pending,
                        UploadedAt = DateTime.UtcNow,
                        Notes = "" // <<< يجب إضافتها
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
