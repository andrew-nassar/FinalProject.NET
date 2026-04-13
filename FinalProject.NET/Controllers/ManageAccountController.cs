using Booking.API.Models;
using FinalProject.NET.Application.Interfaces;
using FinalProject.NET.DBcontext;
using FinalProject.NET.Dtos;
using FinalProject.NET.Services.Cloudinary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace FinalProject.NET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManageAccountController(ILawyerService lawyerService) : ControllerBase
    {
        private readonly ILawyerService _lawyerService = lawyerService;

        [HttpGet("lawyers")]
        public async Task<IActionResult> GetLawyers([FromQuery] LawyerFilterDto filter)
        {
            var res = await _lawyerService.GetLawyersAsync(filter);
            if (!res.Success) return BadRequest(res.Message);
            return Ok(res.Data);
        }

        [HttpGet("lawyer/{id}")]
        public async Task<IActionResult> GetLawyerById(Guid id)
        {
            var res = await _lawyerService.GetLawyerByIdAsync(id);
            if (!res.Success) return NotFound(res.Message);
            return Ok(res.Data);
        }

        [HttpGet("lawyers-for-verification")]
        public async Task<IActionResult> GetLawyersForVerification()
        {
            var res = await _lawyerService.GetLawyersForVerificationAsync();
            if (!res.Success) return BadRequest(res.Message);
            return Ok(res.Data);
        }

        [HttpGet("lawyer-basic/{id}")]
        public async Task<IActionResult> GetLawyerBasic(Guid id)
        {
            var res = await _lawyerService.GetLawyerBasicAsync(id);
            if (!res.Success) return NotFound(res.Message);
            return Ok(res.Data);
        }

        [HttpPatch("update-all-documents-status/{lawyerId}")]
        public async Task<IActionResult> UpdateAllDocumentsStatus(Guid lawyerId, [FromBody] UpdateDocumentsDto dto)
        {
            if (dto == null) return BadRequest("Request body is required");

            var res = await _lawyerService.UpdateAllDocumentsStatusAsync(lawyerId, dto);
            if (!res.Success) return NotFound(res.Message);
            return Ok(res.Data);
        }
        [HttpDelete("lawyer/{id}")]
        public async Task<IActionResult> SoftDeleteLawyer(Guid id)
        {
            var res = await _lawyerService.SoftDeleteLawyerAsync(id);
            if (!res.Success) return NotFound(res.Message);
            return Ok(res.Message);
        }

    }
}
