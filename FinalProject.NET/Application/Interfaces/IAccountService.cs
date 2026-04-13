using System.Threading.Tasks;
using FinalProject.NET.Application.DTOs.Auth;
using FinalProject.NET.Dtos;
using FinalProject.NET.Shared.Models;

namespace FinalProject.NET.Application.Interfaces
{
    public interface IAccountService
    {
        Task<ServiceResponse> GetAllSpecializationsAsync();
        Task<ServiceResponse> RegisterUserAsync(RegisterUserDto dto);
        Task<ServiceResponse> RegisterLawyerAsync(RegisterLawyerDto dto);
        Task<ServiceResponse> ConfirmEmailAsync(string userId, string token);
        Task<ServiceResponse> LoginAsync(LoginDto dto);
        Task<ServiceResponse> SendPasswordResetAsync(ForgotPasswordDto dto);
        Task<ServiceResponse> ResetPasswordAsync(ResetPasswordDto dto);

        Task<ServiceResponse> SendEmailConfirmation(string email);
    }
}
