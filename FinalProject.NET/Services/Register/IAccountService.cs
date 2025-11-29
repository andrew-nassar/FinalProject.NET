using System.Threading.Tasks;
using FinalProject.NET.Dtos;
using FinalProject.NET.Dtos.Auth;

namespace FinalProject.NET.Services.Register
{
    public interface IAccountService
    {
        Task<ServiceResponse> RegisterUserAsync(RegisterUserDto dto);
        Task<ServiceResponse> RegisterLawyerAsync(RegisterLawyerDto dto);
        Task<ServiceResponse> ConfirmEmailAsync(string userId, string token);
        Task<ServiceResponse> LoginAsync(LoginDto dto);
        Task<ServiceResponse> SendPasswordResetAsync(ForgotPasswordDto dto);
        Task<ServiceResponse> ResetPasswordAsync(ResetPasswordDto dto);

        Task<ServiceResponse> SendEmailConfirmation(string email);
    }
}
