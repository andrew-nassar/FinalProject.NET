using Booking.API.Models;
using FinalProject.NET.DBcontext;
using FinalProject.NET.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FinalProject.NET.Application.DTOs.Auth;
using FinalProject.NET.Application.Interfaces;
using FinalProject.NET.Shared.Models;

namespace FinalProject.NET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AuthController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        [HttpPost("register-user")]
        public async Task<IActionResult> RegisterUser([FromForm] RegisterUserDto model)
            => ConvertToHttp(await _accountService.RegisterUserAsync(model));

        [HttpPost("register-lawyer")]
        public async Task<IActionResult> RegisterLawyer([FromForm] RegisterLawyerDto dto)
            => ConvertToHttp(await _accountService.RegisterLawyerAsync(dto));

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
            => ConvertToHttp(await _accountService.ConfirmEmailAsync(userId, token));

        private IActionResult ConvertToHttp(ServiceResponse response)
            => response.Success ? Ok(response) : BadRequest(response);

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var res = await _accountService.SendPasswordResetAsync(dto);
            if (res.Success) return Ok(res);
            return BadRequest(res);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var res = await _accountService.ResetPasswordAsync(dto);
            if (res.Success) return Ok(res);
            return BadRequest(res);
        }
        [HttpPost("send-confirm-email")]
        public async Task<IActionResult> SendComfirmEmail(string email)
            => ConvertToHttp(await _accountService.SendEmailConfirmation(email));
        [HttpGet("specializations")]
        public async Task<IActionResult> GetSpecializations()
        {
            var response = await _accountService.GetAllSpecializationsAsync();

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
    }
}
