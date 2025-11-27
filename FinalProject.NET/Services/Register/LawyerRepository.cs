using FinalProject.NET.DBcontext;
using FinalProject.NET.Dtos;
using FinalProject.NET.Models;
using FinalProject.NET.Services.Register;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace FinalProject.NET.Repositories.Implementations
{
    public class LawyerRepository : ILawyerRepository
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Person> _userManager;


        public LawyerRepository(AppDbContext context, UserManager<Person> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        public async Task<Location> CreateLocationAsync(RegisterLawyerDto dto)
        {
            var loc = new Location
            {
                Country = dto.Location.Country,
                Government = dto.Location.Government,
                City = dto.Location.City,
                Street = dto.Location.Street
            };


            _context.Locations.Add(loc);
            await _context.SaveChangesAsync();
            return loc;
        }
        public async Task<Lawyer> CreateLawyerUserAsync(RegisterLawyerDto dto, Guid locationId)
        {
            var lawyer = new Lawyer
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                UserName = dto.Email,
                OfficeLocationId = locationId
            };


            var result = await _userManager.CreateAsync(lawyer, dto.Password);
            return result.Succeeded ? lawyer : null;
        }
        public async Task CreateLawyerInfoAsync(RegisterLawyerDto dto, Guid lawyerId)
        {
            var info = new LawyerInfo
            {
                Id = lawyerId,
                PhoneNumber = dto.PhoneNumber,
                About = dto.About,
                YearsOfExperience = dto.YearsOfExperience,
            };


            _context.LawyerInfos.Add(info);
            await _context.SaveChangesAsync();
        }
        public async Task AssignSpecializationsAsync(RegisterLawyerDto dto, Guid lawyerId)
        {
            if (dto.Specializations == null || !dto.Specializations.Any()) return;


            var requested = dto.Specializations.Distinct().ToList();


            var validIds = await _context.Specializations
            .Where(s => requested.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync();


            var existing = await _context.LawyerSpecializations
            .Where(ls => ls.LawyerId == lawyerId)
            .Select(ls => ls.SpecializationId)
            .ToListAsync();


            var toAdd = validIds.Except(existing).ToList();
            if (!toAdd.Any()) return;


            var items = toAdd.Select(id => new LawyerSpecialization
            {
                LawyerId = lawyerId,
                SpecializationId = id
            });


            _context.LawyerSpecializations.AddRange(items);
            await _context.SaveChangesAsync();
        }
        public async Task AddDocumentAsync(Guid lawyerId, DocumentType type, string url)
        {
            _context.DocumentVerifications.Add(new DocumentVerification
            {
                Id = Guid.NewGuid(),
                LawyerId = lawyerId,
                DocumentType = type,
                FileUrl = url,
                Status = VerificationStatus.Pending,
                UploadedAt = DateTime.UtcNow,
                Notes = string.Empty
            });


            await _context.SaveChangesAsync();
        }
    }
}
