using SehhaTech.Core.DTOs.Auth;

namespace SehhaTech.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterTenantAsync(RegisterTenantDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task ResetPasswordAsync(ResetPasswordDto dto);
    }
}