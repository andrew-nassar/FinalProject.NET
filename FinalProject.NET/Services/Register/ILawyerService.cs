using FinalProject.NET.DBcontext;
using FinalProject.NET.Dtos;

namespace FinalProject.NET.Services.Register
{
    public interface ILawyerService
    {
        Task<ServiceResponse> RegisterLawyerAsync(RegisterLawyerDto dto);
        Task<ServiceResponse> GetLawyersAsync(LawyerFilterDto filter);
        Task<ServiceResponse> GetLawyerByIdAsync(Guid id);
        Task<ServiceResponse> GetLawyersForVerificationAsync();
        Task<ServiceResponse> GetLawyerBasicAsync(Guid id);
        Task<ServiceResponse> SoftDeleteLawyerAsync(Guid lawyerId);
        Task<ServiceResponse> UpdateAllDocumentsStatusAsync(Guid lawyerId, UpdateDocumentsDto dto);
    }
}
