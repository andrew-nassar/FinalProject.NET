using FinalProject.NET.DBcontext;
using FinalProject.NET.Dtos;
using FinalProject.NET.Services.Cloudinary;

namespace FinalProject.NET.Services.Register
{
    public class LawyerService : ILawyerService
    {
        private readonly ILawyerRepository _repo;
        private readonly ICloudService _cloud;
        private readonly AppDbContext _context;


        public LawyerService(ILawyerRepository repo, ICloudService cloud, AppDbContext context)
        {
            _repo = repo;
            _cloud = cloud;
            _context = context;
        }
        public async Task<ServiceResponse> RegisterLawyerAsync(RegisterLawyerDto dto)
        {
            if (dto.Location == null)
                return ServiceResponse.Fail("Location is required");


            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var location = await _repo.CreateLocationAsync(dto);
                var lawyer = await _repo.CreateLawyerUserAsync(dto, location.Id);
                if (lawyer == null)
                {
                    await tx.RollbackAsync();
                    return ServiceResponse.Fail("Failed to create lawyer user");
                }


                await _repo.CreateLawyerInfoAsync(dto, lawyer.Id);
                await _repo.AssignSpecializationsAsync(dto, lawyer.Id);


                // handle documents
                await HandleDocuments(dto, lawyer.Id);


                await tx.CommitAsync();
                return ServiceResponse.Ok("Lawyer registered successfully", new { lawyer.Id });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return ServiceResponse.Fail("Server error: " + ex.Message);
            }
        }
        private async Task HandleDocuments(RegisterLawyerDto dto, Guid lawyerId)
        {
            var files = new Dictionary<DocumentType, IFormFile>
            {
                { DocumentType.IdFront, dto.IdFront },
                { DocumentType.IdBack, dto.IdBack },
                { DocumentType.SelfieWithId, dto.SelfieWithId },
                { DocumentType.LicensePhoto, dto.LicensePhoto }
            };


            foreach (var kv in files)
            {
                if (kv.Value == null || kv.Value.Length == 0) continue;
                var url = await _cloud.UploadAsync(kv.Value);
                if (!string.IsNullOrEmpty(url))
                    await _repo.AddDocumentAsync(lawyerId, kv.Key, url);
            }
        }
    }
}
