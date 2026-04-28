namespace SehhaTech.Core.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string? MedicalHistory { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Tenant? Tenant { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}