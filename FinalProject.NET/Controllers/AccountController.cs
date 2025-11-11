using System.Text.Json;
using System.Text.RegularExpressions;
using Booking.API.Models;
using FinalProject.NET.DBcontext;
using FinalProject.NET.Dtos;
using FinalProject.NET.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace FinalProject.NET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<Person> _userManager;
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IDistributedCache _cache;

        public AccountController(UserManager<Person> userManager,
                                 AppDbContext context,
                                 IEmailService emailService,
                                 IDistributedCache cache)
        {
            _userManager = userManager;
            _context = context;
            _emailService = emailService;
            _cache = cache;
        }

        // Step1: إرسال الرمز مع تخزين بيانات مؤقتة
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return BadRequest("البريد غير صالح");

            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
                return BadRequest("هذا البريد مستخدم بالفعل");

            var verificationCode = new Random().Next(100000, 999999).ToString();
            var cacheKey = $"verify:{model.Email}";

            var cacheData = new
            {
                model.FirstName,
                model.LastName,
                Code = verificationCode,
                ExpireAt = DateTime.UtcNow.AddMinutes(15)
            };

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(cacheData), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            });

            await _emailService.SendEmailAsync(model.Email, "رمز التحقق", $"رمز التحقق الخاص بك هو: {verificationCode}");

            return Ok(new { success = true, message = "تم إرسال رمز التحقق إلى بريدك الإلكتروني" });
        }

        // Step2: تأكيد الرمز
        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto model)
        {
            var cacheKey = $"verify:{model.Email}";
            var json = await _cache.GetStringAsync(cacheKey);
            if (string.IsNullOrEmpty(json))
                return BadRequest(new { success = false, message = "الرمز غير موجود أو انتهت صلاحيته" });

            var cacheData = JsonSerializer.Deserialize<dynamic>(json);
            if (cacheData.Code != model.Code)
                return BadRequest(new { success = false, message = "رمز التحقق غير صحيح" });

            // نجاح التحقق → يمكن للـ Front الانتقال للخطوة التالية
            return Ok(new { success = true, message = "تم التحقق من البريد. يمكنك الآن إكمال التسجيل." });
        }


        // Step3: إنشاء الحساب بعد التأكد من البريد
        [HttpPost("register")]
        public async Task<IActionResult> CompleteRegister([FromBody] RegisterCompleteDto model)
        {
            // تحقق من البريد
            var savedCode = await _cache.GetStringAsync(model.Email);
            if (string.IsNullOrEmpty(savedCode) || savedCode != model.VerificationCode)
                return BadRequest("التحقق من البريد لم يتم أو انتهت صلاحيته");

            // تحقق من الوثائق
            var requiredDocs = model.Role == Role.Lawyer
                ? new[] { DocumentType.IdFront, DocumentType.IdBack, DocumentType.SelfieWithId, DocumentType.LicensePhoto }
                : new[] { DocumentType.IdFront, DocumentType.IdBack, DocumentType.SelfieWithId };

            var uploadedTypes = model.Documents?.Select(d => d.DocumentType).ToList() ?? new List<DocumentType>();
            var missingDocs = requiredDocs.Except(uploadedTypes).ToList();
            if (missingDocs.Any())
                return BadRequest($"الوثائق التالية ناقصة: {string.Join(", ", missingDocs)}");

            if (model.Role == Role.Lawyer && (model.Specializations == null || !model.Specializations.Any()))
                return BadRequest("يجب اختيار تخصص واحد على الأقل");

            // إنشاء الحساب
            Person person = model.Role == Role.Lawyer ? new Lawyer() : new User();
            person.FirstName = model.FirstName;
            person.LastName = model.LastName;
            person.Email = model.Email;
            person.UserName = model.Email;
            person.Role = model.Role;
            person.AccountStatus = AccountStatus.Pending;

            var result = await _userManager.CreateAsync(person, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            // حفظ الوثائق
            var documentEntities = model.Documents.Select(d => new DocumentVerification
            {
                PersonId = person.Id,
                DocumentType = d.DocumentType,
                FilePath = d.FilePath,
                Status = VerificationStatus.Pending,
                UploadedAt = DateTime.UtcNow
            }).ToList();
            await _context.DocumentVerifications.AddRangeAsync(documentEntities);

            // حفظ التخصصات للمحامي
            if (person is Lawyer lawyer)
            {
                lawyer.LawyerSpecializations = model.Specializations.Select(s => new LawyerSpecialization
                {
                    LawyerId = lawyer.Id,
                    SpecializationId = s
                }).ToList();
            }

            await _context.SaveChangesAsync();
            return Ok("تم إنشاء الحساب بنجاح بعد التحقق من البريد وتحميل كل البيانات المطلوبة");
        }

        
    }
}
