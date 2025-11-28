using Booking.API.Models;
using FinalProject.NET.DBcontext;
using FinalProject.NET.Dtos;
using FinalProject.NET.Models;
using FinalProject.NET.Services.Email;
using FinalProject.NET.Services.Middleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.NET.Services.Register
{

    public class AccountService : IAccountService
    {
        private readonly UserManager<Person> _userManager;
        private readonly ILawyerService _lawyerService;
        private readonly IEmailSenderService _emailSender;
        private readonly SignInManager<Person> _signInManager;
        private readonly JwtTokenService _jwtTokenService;
        private readonly AppDbContext _context;

        public AccountService(UserManager<Person> userManager, ILawyerService lawyerService,
              IEmailSenderService emailSender, SignInManager<Person> signInManager,
        JwtTokenService jwtTokenService, AppDbContext context)
        {
            _userManager = userManager;
            _lawyerService = lawyerService;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _context = context;
        }
        public async Task<ServiceResponse> RegisterUserAsync(RegisterUserDto dto)
        {
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                UserName = dto.Email,
            };


            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return ServiceResponse.Fail(string.Join("; ", result.Errors));


            // Send confirmation
            await _emailSender.SendEmailConfirmationAsync(user);
            return ServiceResponse.Ok("User created. Check email for confirmation.");
        }
        public async Task<ServiceResponse> RegisterLawyerAsync(RegisterLawyerDto dto)
            => await _lawyerService.RegisterLawyerAsync(dto);

        public async Task<ServiceResponse> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return ServiceResponse.Fail("Invalid email or password");

            if (!user.EmailConfirmed)
                return ServiceResponse.Fail("Email not confirmed");

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                return ServiceResponse.Fail("Invalid email or password");

            // لو المستخدم محامي تحقق من مستنداته
            if (user.Role == Role.Lawyer)
            {
                var lawyer = await _context.Lawyers
                    .Include(l => l.Documents)
                    .FirstOrDefaultAsync(l => l.Id == user.Id);

                if (lawyer == null)
                    return ServiceResponse.Fail("Lawyer profile not found");

                // تحقق إن كل المستندات Approved
                if (!lawyer.Documents.Any() || lawyer.Documents.Any(d => d.Status != VerificationStatus.Approved))
                    return ServiceResponse.Fail("Your documents are under review or not approved yet");
            }
            var token = _jwtTokenService.GenerateToken(user.Id.ToString(), user.Role.ToString());

            var response = new LoginResponseDto
            {
                Token = token,
                Role = user.Role.ToString(),
            };

            return ServiceResponse.Ok("Login successful", data: response);
        }
        public async Task<ServiceResponse> ConfirmEmailAsync(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return ServiceResponse.Fail("Invalid parameters");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return ServiceResponse.Fail("User not found");

            var decoded = Uri.UnescapeDataString(token);
            var result = await _userManager.ConfirmEmailAsync(user, decoded);

            if (result.Succeeded && user.Role == Role.Lawyer)
                await _emailSender.SendPendingReviewMessage(user);

            return result.Succeeded
                ? ServiceResponse.Ok("Email confirmed")
                : ServiceResponse.Fail("Invalid token or failed");
        }
    }

}
