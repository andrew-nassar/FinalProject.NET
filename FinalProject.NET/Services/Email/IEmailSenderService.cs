using FinalProject.NET.Models;

namespace FinalProject.NET.Services.Email
{
    public interface IEmailSenderService
    {
        Task SendEmailConfirmationAsync(Person user);
        Task SendPendingReviewMessage(Person user);
        Task SendPasswordResetAsync(string email, string resetLink);
    }
}
