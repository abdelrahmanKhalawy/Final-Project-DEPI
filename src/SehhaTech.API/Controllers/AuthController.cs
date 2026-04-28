using Microsoft.AspNetCore.Mvc;
using SehhaTech.Core.DTOs.Auth;
using SehhaTech.Core.Interfaces;

namespace SehhaTech.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterTenantDto dto)
        {
            try
            {
                var result = await _authService.RegisterTenantAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var result = await _authService.LoginAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            try
            {
                await _authService.ResetPasswordAsync(dto);
                return Ok(new { message = "Password reset successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}