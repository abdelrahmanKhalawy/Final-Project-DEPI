namespace SehhaTech.Core.Models
{
    public class Tenant
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool IsActive { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Subscription? Subscription { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}