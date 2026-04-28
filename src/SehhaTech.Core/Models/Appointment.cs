namespace SehhaTech.Core.Models
{
    public enum AppointmentStatus
    {
        Scheduled,
        Confirmed,
        Completed,
        Cancelled,
        NoShow
    }

    public class Appointment
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan Duration { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Tenant? Tenant { get; set; }
        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
    }
}