namespace SehhaTech.Core.DTOs.Auth
{
    public class RegisterTenantDto
    {
        public string ClinicName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
    }
}