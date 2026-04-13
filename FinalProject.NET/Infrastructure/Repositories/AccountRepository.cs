using FinalProject.NET.Application.Interfaces;
using FinalProject.NET.Infrastructure.Data.Entities;
using FinalProject.NET.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.NET.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly UserManager<Person> _userManager;
        private readonly AppDbContext _context;

        public AccountRepository(UserManager<Person> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public async Task<List<Specialization>> GetAllSpecializationsAsync()
        {
            return await _context.Specializations.ToListAsync();
        }
        public async Task<User?> FindByEmailAsync(string email)
            => await _userManager.FindByEmailAsync(email) as User;

        public async Task<User?> FindByIdAsync(string id)
            => await _userManager.FindByIdAsync(id) as User;

        public async Task<IdentityResult> CreateUserAsync(User user, string password)
            => await _userManager.CreateAsync(user, password);

        public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
            => await _userManager.ConfirmEmailAsync(user, token);

        public async Task<string> GeneratePasswordResetTokenAsync(User user)
            => await _userManager.GeneratePasswordResetTokenAsync(user);

        public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword)
            => await _userManager.ResetPasswordAsync(user, token, newPassword);

        public async Task<Lawyer?> GetLawyerWithDocumentsAsync(Guid lawyerId)
        {
            return await _context.Lawyers
                .Include(l => l.Documents)
                .FirstOrDefaultAsync(l => l.Id == lawyerId);
        }
    }
}
