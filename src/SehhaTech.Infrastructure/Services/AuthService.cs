using Microsoft.EntityFrameworkCore;
using SehhaTech.Core.DTOs.Auth;
using SehhaTech.Core.Interfaces;
using SehhaTech.Core.Models;
using SehhaTech.Infrastructure.Data;

namespace SehhaTech.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthService(AppDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<AuthResponseDto> RegisterTenantAsync(RegisterTenantDto dto)
        {
            // تأكد إن الإيميل مش موجود
            var existingUser = await _context.Users
                .AnyAsync(u => u.Email == dto.Email);

            if (existingUser)
            {
                var existingUserData = await _context.Users
                    .Include(u => u.Tenant)
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (existingUserData?.Tenant != null && !existingUserData.Tenant.IsActive)
                {
                    var existingToken = _jwtService.GenerateToken(existingUserData);
                    return new AuthResponseDto
                    {
                        Token = existingToken,
                        FullName = existingUserData.FullName,
                        Email = existingUserData.Email,
                        Role = existingUserData.Role.ToString(),
                        TenantId = existingUserData.TenantId,
                        MustResetPassword = existingUserData.MustResetPassword
                    };
                }

                throw new Exception("Email already exists");
            }

            // إنشاء التيننت
            var tenant = new Tenant
            {
                Name = dto.ClinicName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                Specialization = dto.Specialization,
                IsActive = false
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            // إنشاء الـ Admin
            var admin = new User
            {
                TenantId = tenant.Id,
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = UserRole.ClinicAdmin,
                MustResetPassword = false
            };

            _context.Users.Add(admin);
            await _context.SaveChangesAsync();

            // Generate Token
            var token = _jwtService.GenerateToken(admin);

            return new AuthResponseDto
            {
                Token = token,
                FullName = admin.FullName,
                Email = admin.Email,
                Role = admin.Role.ToString(),
                TenantId = admin.TenantId,
                MustResetPassword = admin.MustResetPassword
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            // جيب الـ User
            var user = await _context.Users
                .Include(u => u.Tenant)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                throw new Exception("Invalid email or password");

            // تأكد من الباسورد
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new Exception("Invalid email or password");

            // تأكد إن الـ User Active
            if (!user.IsActive)
                throw new Exception("Your account is inactive");

            // لو مش SuperAdmin تأكد إن العيادة Active
            if (user.Role != UserRole.SuperAdmin)
            {
                if (user.Tenant == null || !user.Tenant.IsActive)
                    throw new Exception("Your clinic subscription is inactive");
            }

            var token = _jwtService.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                TenantId = user.TenantId,
                MustResetPassword = user.MustResetPassword
            };
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                throw new Exception("Passwords do not match");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                throw new Exception("User not found");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.MustResetPassword = false;

            await _context.SaveChangesAsync();
        }
    }
}