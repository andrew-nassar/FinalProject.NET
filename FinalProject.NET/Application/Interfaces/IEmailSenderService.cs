using FinalProject.NET.Infrastructure.Data.Entities;

namespace FinalProject.NET.Application.Interfaces
{
    public interface IEmailSenderService
    {
        Task SendEmailConfirmationAsync(Person user);
        Task SendPendingReviewMessage(Person user);
        Task SendPasswordResetAsync(string email, string resetLink);
    }
}
