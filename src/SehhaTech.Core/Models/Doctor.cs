namespace SehhaTech.Core.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public int UserId { get; set; }
        public string Specialization { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? ProfileImageUrl { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public Tenant? Tenant { get; set; }
        public User? User { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}