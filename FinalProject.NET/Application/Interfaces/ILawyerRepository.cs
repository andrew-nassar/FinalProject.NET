using FinalProject.NET.DBcontext;
using FinalProject.NET.Dtos;
using FinalProject.NET.Infrastructure.Data.Entities;

namespace FinalProject.NET.Application.Interfaces
{
    public interface ILawyerRepository
    {
        Task<Location> CreateLocationAsync(RegisterLawyerDto dto);
        Task<Lawyer> CreateLawyerUserAsync(RegisterLawyerDto dto, Guid locationId);
        Task CreateLawyerInfoAsync(RegisterLawyerDto dto, Guid lawyerId);
        Task AssignSpecializationsAsync(RegisterLawyerDto dto, Guid lawyerId);
        Task AddDocumentAsync(Guid lawyerId, DocumentType type, string url);
    }
}
