using Booking.API.Models;
using FinalProject.NET.DBcontext;
using FinalProject.NET.Dtos;
using FinalProject.NET.Models;
using FinalProject.NET.Services.Cloudinary;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace FinalProject.NET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManageAccountController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ManageAccountController(
            AppDbContext context)
        {
            _context = context;
        }
        [HttpGet("lawyers")]
        public async Task<IActionResult> GetLawyers([FromQuery] LawyerFilterDto filter)
        {
            var query = _context.Lawyers
                .Include(l => l.OfficeLocation)
                .Include(l => l.LawyerSpecializations)
                    .ThenInclude(ls => ls.Specialization)
                .Where(l => !l.IsDeleted && l.AccountStatus == AccountStatus.Active) // NEW
                .AsQueryable();
            query = query.Where(l =>
                l.Documents.All(d => d.Status == VerificationStatus.Approved));


            // --------------------- 1) Filter by Specializations ---------------------
            if (filter.SpecializationIds != null && filter.SpecializationIds.Any())
            {
                query = query.Where(l =>
                    l.LawyerSpecializations.Any(ls =>
                        filter.SpecializationIds.Contains(ls.SpecializationId)));
            }

            // --------------------- 2) Filter by Location ---------------------
            if (!string.IsNullOrEmpty(filter.Country))
                query = query.Where(l => l.OfficeLocation.Country == filter.Country);

            if (!string.IsNullOrEmpty(filter.Government))
                query = query.Where(l => l.OfficeLocation.Government == filter.Government);

            if (!string.IsNullOrEmpty(filter.City))
                query = query.Where(l => l.OfficeLocation.City == filter.City);

            // --------------------- Execute Query ---------------------
            var lawyers = await query.Select(l => new
            {
                l.Id,
                FullName = l.FirstName + " " + l.LastName,
                l.Email,
                l.PhoneNumber,
                l.About,
                l.YearsOfExperience,

                Location = new
                {
                    l.OfficeLocation.Country,
                    l.OfficeLocation.Government,
                    l.OfficeLocation.City,
                    l.OfficeLocation.Street
                },

                Specializations = l.LawyerSpecializations
                    .Select(ls => ls.Specialization.Name)
                    .ToList(),

            }).ToListAsync();

            return Ok(lawyers);
        }

    }
}
