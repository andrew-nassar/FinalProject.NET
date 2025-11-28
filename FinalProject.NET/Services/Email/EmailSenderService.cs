using Booking.API.Models;
using FinalProject.NET.Models;
using Microsoft.AspNetCore.Identity;

namespace FinalProject.NET.Services.Email
{
    public class EmailSenderService : IEmailSenderService
    {
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<Person> _userManager;

        public EmailSenderService(
            IEmailService emailService,
            IWebHostEnvironment env,
            UserManager<Person> userManager)
        {
            _emailService = emailService;
            _env = env;
            _userManager = userManager;
        }

        #region Email Confirmation

        public async Task SendEmailConfirmationAsync(Person user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encoded = Uri.EscapeDataString(token);
            var link = $"{GetBaseUrl()}/api/AccountII/confirm-email?userId={user.Id}&token={encoded}";

            var path = Path.Combine(_env.ContentRootPath, "EmailTemplates", "EmailConfirmation.html");
            if (!File.Exists(path)) return;

            var html = await File.ReadAllTextAsync(path);
            html = html.Replace("{{CONFIRMATION_LINK}}", link);

            await _emailService.SendEmailAsync(user.Email, "Confirm your account", html);
        }

        #endregion

        #region Pending Review

        public async Task SendPendingReviewMessage(Person user)
        {
            var path = Path.Combine(_env.ContentRootPath, "EmailTemplates", "LawyerPendingActivation.html");
            if (!File.Exists(path)) return;

            var html = await File.ReadAllTextAsync(path);
            await _emailService.SendEmailAsync(
                user.Email,
                "Your account is under review",
                html
            );
        }

        #endregion

        #region Helpers

        private string GetBaseUrl()
        {
            return "https://localhost:5001";
        }

        #endregion
    }
}
