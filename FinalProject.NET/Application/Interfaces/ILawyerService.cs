using FinalProject.NET.DBcontext;
using FinalProject.NET.Dtos;
using FinalProject.NET.Shared.Models;

namespace FinalProject.NET.Application.Interfaces
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
