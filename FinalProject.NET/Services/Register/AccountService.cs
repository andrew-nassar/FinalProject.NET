using System.Text;
using Booking.API.Models;
using FinalProject.NET.DBcontext;
using FinalProject.NET.Dtos;
using FinalProject.NET.Dtos.Auth;
using FinalProject.NET.Models;
using FinalProject.NET.Services.Email;
using FinalProject.NET.Services.Middleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
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

        // inside AccountService class
        public async Task<ServiceResponse> SendEmailConfirmation(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return ServiceResponse.Fail("Email is required");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return ServiceResponse.Fail("User not found");

            if (user.EmailConfirmed)
                return ServiceResponse.Fail("Email already confirmed");

            // Send confirmation
            await _emailSender.SendEmailConfirmationAsync(user);

            return ServiceResponse.Ok("Confirmation email sent");
        }

        public async Task<ServiceResponse> SendPasswordResetAsync(ForgotPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return ServiceResponse.Fail("Email is required");

            var user = await _userManager.FindByEmailAsync(dto.Email);

            // Important: Don't reveal whether the email exists
            if (user == null)
                return ServiceResponse.Ok("If an account with that email exists, a reset link has been sent.");

            // Generate reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Encode token as Base64Url
            var tokenBytes = Encoding.UTF8.GetBytes(token);
            var encodedToken = WebEncoders.Base64UrlEncode(tokenBytes);

            // Build front-end reset URL
            var frontendResetUrl = "https://your-frontend.com/reset-password";

            var resetLink =
                $"{frontendResetUrl}?userId={Uri.EscapeDataString(user.Id.ToString())}&token={Uri.EscapeDataString(encodedToken)}";

            // Use email service
            await _emailSender.SendPasswordResetAsync(user.Email, resetLink);

            return ServiceResponse.Ok("If an account with that email exists, a reset link has been sent.");
        }


        public async Task<ServiceResponse> ResetPasswordAsync(ResetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserId) ||
                string.IsNullOrWhiteSpace(dto.Token) ||
                string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return ServiceResponse.Fail("Invalid request");
            }

            var user = await _userManager.FindByIdAsync(dto.UserId);

            if (user == null)
                return ServiceResponse.Fail("Invalid token or user");

            try
            {
                // Decode the token (Base64Url)
                var tokenBytes = WebEncoders.Base64UrlDecode(dto.Token);
                var decodedToken = Encoding.UTF8.GetString(tokenBytes);

                // Reset the password
                var result = await _userManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);

                if (!result.Succeeded)
                    return ServiceResponse.Fail(string.Join("; ", result.Errors.Select(e => e.Description)));

                return ServiceResponse.Ok("Password has been reset successfully.");
            }
            catch
            {
                return ServiceResponse.Fail("Invalid token format");
            }
        }
    }

}
