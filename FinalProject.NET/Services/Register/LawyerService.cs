using FinalProject.NET.DBcontext;
using FinalProject.NET.Dtos;
using FinalProject.NET.Models;
using FinalProject.NET.Services.Cloudinary;
using FinalProject.NET.Services.Email;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.NET.Services.Register
{
    public class LawyerService : ILawyerService
    {
        private readonly ILawyerRepository _repo;
        private readonly ICloudService _cloud;
        private readonly AppDbContext _context;
        private readonly IGenericRepository<Lawyer> _lawyerRepo;
        private readonly IGenericRepository<DocumentVerification> _docRepo;
        private readonly IEmailSenderService _emailSender;

        public LawyerService(ILawyerRepository repo, ICloudService cloud, AppDbContext context,
            IEmailSenderService emailSender, IGenericRepository<Lawyer> lawyerRepo,
                             IGenericRepository<DocumentVerification> docRepo)
        {
            _repo = repo;
            _cloud = cloud;
            _context = context;
            _lawyerRepo = lawyerRepo;
            _docRepo = docRepo;
            _emailSender = emailSender;

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

                await _emailSender.SendEmailConfirmationAsync(lawyer);
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
        public async Task<ServiceResponse> GetLawyersAsync(LawyerFilterDto filter)
        {
            var query = _lawyerRepo.Query()
                .Include(l => l.OfficeLocation)
                .Include(l => l.Documents)
                .Include(l => l.LawyerSpecializations)
                    .ThenInclude(ls => ls.Specialization)
                .Where(l => !l.IsDeleted && l.EmailConfirmed)
                .AsQueryable();

            query = query.Where(l => l.Documents.Any() && l.Documents.All(d => d.Status == VerificationStatus.Approved));

            if (filter?.SpecializationIds != null && filter.SpecializationIds.Any())
            {
                query = query.Where(l =>
                    l.LawyerSpecializations.Any(ls =>
                        filter.SpecializationIds.Contains(ls.SpecializationId)));
            }

            if (!string.IsNullOrEmpty(filter?.Country))
                query = query.Where(l => l.OfficeLocation.Country == filter.Country);

            if (!string.IsNullOrEmpty(filter?.Government))
                query = query.Where(l => l.OfficeLocation.Government == filter.Government);

            if (!string.IsNullOrEmpty(filter?.City))
                query = query.Where(l => l.OfficeLocation.City == filter.City);

            var lawyers = await query.Select(l => new
            {
                l.Id,
                FullName = l.FirstName + " " + l.LastName,
                l.Email,
                PhoneNumber = l.LawyerInfo.PhoneNumber,
                About = l.LawyerInfo.About,
                Experience = l.LawyerInfo.YearsOfExperience,
                Location = new
                {
                    l.OfficeLocation.Country,
                    l.OfficeLocation.Government,
                    l.OfficeLocation.City,
                    l.OfficeLocation.Street
                },
                Specializations = l.LawyerSpecializations.Select(ls => ls.Specialization.Name).ToList()
            }).ToListAsync();

            return ServiceResponse.Ok("Fetched lawyers", lawyers);
        }

        public async Task<ServiceResponse> GetLawyerByIdAsync(Guid id)
        {
            var lawyer = await _lawyerRepo.Query()
                .Include(l => l.LawyerInfo)
                .Include(l => l.OfficeLocation)
                .Include(l => l.LawyerSpecializations)
                    .ThenInclude(ls => ls.Specialization)
                .Where(l => !l.IsDeleted && l.EmailConfirmed && l.Id == id)
                .Select(l => new
                {
                    l.Id,
                    FullName = l.FirstName + " " + l.LastName,
                    l.Email,
                    Info = new
                    {
                        l.LawyerInfo.PhoneNumber,
                        l.LawyerInfo.About,
                        l.LawyerInfo.YearsOfExperience,
                        l.LawyerInfo.Personal_Photo_Url
                    },
                    Location = new
                    {
                        l.OfficeLocation.Country,
                        l.OfficeLocation.Government,
                        l.OfficeLocation.City,
                        l.OfficeLocation.Street
                    },
                    Specializations = l.LawyerSpecializations.Select(ls => ls.Specialization.Name).ToList()
                })
                .FirstOrDefaultAsync();

            if (lawyer == null)
                return ServiceResponse.Fail("Lawyer not found");

            return ServiceResponse.Ok("Found", lawyer);
        }

        public async Task<ServiceResponse> GetLawyersForVerificationAsync()
        {
            var lawyers = await _lawyerRepo.Query()
                .Include(l => l.Documents)
                .Where(l => !l.IsDeleted && l.EmailConfirmed && l.Documents.Any(d => d.Status != VerificationStatus.Approved))
                .Select(l => new
                {
                    l.Id,
                    FullName = l.FirstName + " " + l.LastName,
                    Email = l.Email,
                    Status = "PendingReview"
                })
                .ToListAsync();

            return ServiceResponse.Ok("Lawyers for verification", lawyers);
        }

        public async Task<ServiceResponse> GetLawyerBasicAsync(Guid id)
        {
            var lawyer = await _lawyerRepo.Query()
                .Include(l => l.LawyerInfo)
                .Include(l => l.OfficeLocation)
                .Include(l => l.LawyerSpecializations)
                    .ThenInclude(ls => ls.Specialization)
                .Where(l => !l.IsDeleted && l.EmailConfirmed && l.Id == id)
                .Select(l => new
                {
                    l.Id,
                    FullName = l.FirstName + " " + l.LastName,
                    l.Email,
                    Info = new
                    {
                        l.LawyerInfo.PhoneNumber,
                        l.LawyerInfo.About,
                        l.LawyerInfo.YearsOfExperience,
                        l.LawyerInfo.Personal_Photo_Url
                    },
                    Location = new
                    {
                        l.OfficeLocation.Country,
                        l.OfficeLocation.Government,
                        l.OfficeLocation.City,
                        l.OfficeLocation.Street
                    },
                    Specializations = l.LawyerSpecializations.Select(ls => ls.Specialization.Name)
                })
                .FirstOrDefaultAsync();

            if (lawyer == null) return ServiceResponse.Fail("Lawyer not found");
            return ServiceResponse.Ok("Found", lawyer);
        }
        public async Task<ServiceResponse> SoftDeleteLawyerAsync(Guid lawyerId)
        {
            var lawyer = await _lawyerRepo.GetByIdAsync(lawyerId);
            if (lawyer == null) return ServiceResponse.Fail("Lawyer not found");

            var success = await _lawyerRepo.SoftDeleteAsync(lawyer);
            if (!success) return ServiceResponse.Fail("Failed to soft delete");

            return ServiceResponse.Ok("Lawyer soft-deleted");
        }
        public async Task<ServiceResponse> UpdateAllDocumentsStatusAsync(Guid lawyerId, UpdateDocumentsDto dto)
        {
            var docs = await _docRepo.GetAsync(d => d.LawyerId == lawyerId);
            if (!docs.Any()) return ServiceResponse.Fail("No documents found for this lawyer");

            foreach (var doc in docs)
            {
                doc.Status = dto.Status;
                doc.ReviewedAt = DateTime.UtcNow;
            }

            await _docRepo.SaveChangesAsync();

            return ServiceResponse.Ok("All documents updated", new
            {
                LawyerId = lawyerId,
                Status = dto.Status.ToString()
            });
        }

    }
}
