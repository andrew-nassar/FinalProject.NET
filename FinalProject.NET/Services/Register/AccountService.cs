using Booking.API.Models;
using FinalProject.NET.Dtos;
using FinalProject.NET.Models;
using Microsoft.AspNetCore.Identity;

namespace FinalProject.NET.Services.Register
{

    public class AccountService : IAccountService
    {
        private readonly UserManager<Person> _userManager;
        private readonly ILawyerService _lawyerService;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _env;


        public AccountService(UserManager<Person> userManager, ILawyerService lawyerService, IEmailService emailService, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _lawyerService = lawyerService;
            _emailService = emailService;
            _env = env;
        }
        public async Task<ServiceResponse> RegisterUserAsync(RegisterUserDto dto)
        {
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                UserName = dto.Email,
                AccountStatus = DBcontext.AccountStatus.Active
            };


            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return ServiceResponse.Fail(string.Join("; ", result.Errors));


            // Send confirmation
            await SendEmailConfirmationAsync(user);
            return ServiceResponse.Ok("User created. Check email for confirmation.");
        }
        public async Task<ServiceResponse> RegisterLawyerAsync(RegisterLawyerDto dto)
            => await _lawyerService.RegisterLawyerAsync(dto);


        public async Task<ServiceResponse> ConfirmEmailAsync(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return ServiceResponse.Fail("Invalid parameters");


            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return ServiceResponse.Fail("User not found");


            var decoded = Uri.UnescapeDataString(token);
            var result = await _userManager.ConfirmEmailAsync(user, decoded);


            return result.Succeeded ? ServiceResponse.Ok("Email confirmed") : ServiceResponse.Fail("Invalid token or failed");
        }


        private async Task SendEmailConfirmationAsync(Person user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encoded = Uri.EscapeDataString(token);
            var link = $"{GetBaseUrl()}/api/auth/account/confirm-email?userId={user.Id}&token={encoded}";


            var path = Path.Combine(_env.ContentRootPath, "EmailTemplates", "EmailConfirmation.html");
            if (!File.Exists(path)) return;


            var html = await File.ReadAllTextAsync(path);
            html = html.Replace("{{CONFIRMATION_LINK}}", link);
            await _emailService.SendEmailAsync(user.Email, "Confirm your account", html);
        }


        private string GetBaseUrl()
        {
            // You can improve this to read from config
            return "https://localhost:5001";
        }
    }

}
