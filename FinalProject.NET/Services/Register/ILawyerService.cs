using FinalProject.NET.Dtos;

namespace FinalProject.NET.Services.Register
{
    public interface ILawyerService
    {
        Task<ServiceResponse> RegisterLawyerAsync(RegisterLawyerDto dto);
    }
}
