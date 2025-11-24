using FinalProject.NET.Dtos;
using FinalProject.NET.Models;

namespace FinalProject.NET.Services.Register
{
    public interface ILawyerRepository
    {
        Task<Location> CreateLocationAsync(RegisterLawyerDto dto);
        Task<Lawyer> CreateLawyerUserAsync(RegisterLawyerDto dto, Guid locationId);
        Task CreateLawyerInfoAsync(RegisterLawyerDto dto, Guid lawyerId);
        Task AssignSpecializationsAsync(RegisterLawyerDto dto, Guid lawyerId);
        Task AddDocumentAsync(Guid lawyerId, DBcontext.DocumentType type, string url);
    }
}
