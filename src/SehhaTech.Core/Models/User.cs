namespace SehhaTech.Core.Models
{
    public enum UserRole
    {
        SuperAdmin,
        ClinicAdmin,
        Doctor,
        Reception
    }

    public class User
    {
        public int Id { get; set; }
        public int? TenantId { get; set; }        // null if SuperAdmin
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool MustResetPassword { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public string? ProfileImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Tenant? Tenant { get; set; }
    }
}