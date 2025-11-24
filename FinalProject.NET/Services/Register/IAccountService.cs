using FinalProject.NET.Dtos;

namespace FinalProject.NET.Services.Register
{
    public interface IAccountService
    {
        Task<ServiceResponse> RegisterUserAsync(RegisterUserDto dto);
        Task<ServiceResponse> RegisterLawyerAsync(RegisterLawyerDto dto);
        Task<ServiceResponse> ConfirmEmailAsync(string userId, string token);
    }
}
