using System.Text;
using Booking.API.Models;
using FinalProject.NET.Application.DTOs.Auth;
using FinalProject.NET.Application.DTOs.Lawyer___User;
using FinalProject.NET.Application.Interfaces;
using FinalProject.NET.DBcontext;
using FinalProject.NET.Dtos;
using FinalProject.NET.Infrastructure.Data;
using FinalProject.NET.Infrastructure.Data.Entities;
using FinalProject.NET.Shared.Middleware;
using FinalProject.NET.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.NET.Services.Register
{

    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepo;
        private readonly ILawyerService _lawyerService;
        private readonly IEmailSenderService _emailSender;
        private readonly SignInManager<Person> _signInManager;
        private readonly JwtTokenService _jwtTokenService;

        public AccountService(
            IAccountRepository accountRepo,
            ILawyerService lawyerService,
            IEmailSenderService emailSender,
            SignInManager<Person> signInManager,
            JwtTokenService jwtTokenService)
        {
            _accountRepo = accountRepo;
            _lawyerService = lawyerService;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
        }
        public async Task<ServiceResponse> GetAllSpecializationsAsync()
        {
            var data = await _accountRepo.GetAllSpecializationsAsync();

            if (data == null || !data.Any())
                return ServiceResponse.Fail("No specializations found");

            var result = data.Select(s => new SpecializationDto
            {
                Id = s.Id,
                Name = s.Name
            }).ToList();

            return ServiceResponse.Ok("Success", result);
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

            var result = await _accountRepo.CreateUserAsync(user, dto.Password);

            if (!result.Succeeded)
                return ServiceResponse.Fail(string.Join("; ", result.Errors.Select(e => e.Description)));

            await _emailSender.SendEmailConfirmationAsync(user);
            return ServiceResponse.Ok("User created. Check email for confirmation.");
        }

        public async Task<ServiceResponse> RegisterLawyerAsync(RegisterLawyerDto dto)
            => await _lawyerService.RegisterLawyerAsync(dto);
        public async Task<ServiceResponse> LoginAsync(LoginDto dto)
        {
            var user = await _accountRepo.FindByEmailAsync(dto.Email);
            if (user == null) return ServiceResponse.Fail("Invalid email or password");

            if (!user.EmailConfirmed) return ServiceResponse.Fail("Email not confirmed");

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded) return ServiceResponse.Fail("Invalid email or password");

            // Lawyer specific verification
            if (user.Role == Role.Lawyer)
            {
                var lawyer = await _accountRepo.GetLawyerWithDocumentsAsync(user.Id);
                if (lawyer == null) return ServiceResponse.Fail("Lawyer profile not found");

                if (!lawyer.Documents.Any() || lawyer.Documents.Any(d => d.Status != VerificationStatus.Approved))
                    return ServiceResponse.Fail("Your documents are under review or not approved yet");
            }

            var token = _jwtTokenService.GenerateToken(user.Id.ToString(), user.Role.ToString());
            return ServiceResponse.Ok("Login successful", data: new LoginResponseDto { Token = token, Role = user.Role.ToString() });
        }

        public async Task<ServiceResponse> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _accountRepo.FindByIdAsync(userId);
            if (user == null) return ServiceResponse.Fail("User not found");

            var decodedToken = Uri.UnescapeDataString(token);
            var result = await _accountRepo.ConfirmEmailAsync(user, decodedToken);

            if (result.Succeeded && user.Role == Role.Lawyer)
                await _emailSender.SendPendingReviewMessage(user);

            return result.Succeeded
                ? ServiceResponse.Ok("Email confirmed")
                : ServiceResponse.Fail("Invalid token or failed");
        }

        public async Task<ServiceResponse> SendPasswordResetAsync(ForgotPasswordDto dto)
        {
            var user = await _accountRepo.FindByEmailAsync(dto.Email);
            if (user == null) return ServiceResponse.Ok("If an account exists, a reset link has been sent.");

            var token = await _accountRepo.GeneratePasswordResetTokenAsync(user);

            // Encode for URL safety
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var resetLink = $"https://your-frontend.com/reset-password?userId={user.Id}&token={encodedToken}";

            await _emailSender.SendPasswordResetAsync(user.Email, resetLink);
            return ServiceResponse.Ok("If an account exists, a reset link has been sent.");
        }

        public async Task<ServiceResponse> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _accountRepo.FindByIdAsync(dto.UserId);
            if (user == null) return ServiceResponse.Fail("Invalid token or user");

            try
            {
                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
                var result = await _accountRepo.ResetPasswordAsync(user, decodedToken, dto.NewPassword);

                return result.Succeeded
                    ? ServiceResponse.Ok("Password reset successfully.")
                    : ServiceResponse.Fail(string.Join("; ", result.Errors.Select(e => e.Description)));
            }
            catch { return ServiceResponse.Fail("Invalid token format"); }
        }
        // inside AccountService class
        public async Task<ServiceResponse> SendEmailConfirmation(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return ServiceResponse.Fail("Email is required");

            var user = await _accountRepo.FindByEmailAsync(email);
            if (user == null)
                return ServiceResponse.Fail("User not found");

            if (user.EmailConfirmed)
                return ServiceResponse.Fail("Email already confirmed");

            // Send confirmation
            await _emailSender.SendEmailConfirmationAsync(user);

            return ServiceResponse.Ok("Confirmation email sent");
        }

    }

}
