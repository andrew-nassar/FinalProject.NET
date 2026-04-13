using FinalProject.NET.Infrastructure.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace FinalProject.NET.Application.Interfaces
{
    public interface IAccountRepository
    {
        Task<List<Specialization>> GetAllSpecializationsAsync();

        Task<User?> FindByEmailAsync(string email);
        Task<User?> FindByIdAsync(string id);
        Task<IdentityResult> CreateUserAsync(User user, string password);
        Task<IdentityResult> ConfirmEmailAsync(User user, string token);
        Task<string> GeneratePasswordResetTokenAsync(User user);
        Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword);

        // Specifically for the Lawyer documents check in Login
        Task<Lawyer?> GetLawyerWithDocumentsAsync(Guid lawyerId);
    }
}
