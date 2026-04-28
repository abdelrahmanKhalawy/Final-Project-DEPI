namespace SehhaTech.Core.Models
{

    public enum SubscriptionStatus
    {
        Pending,
        Active,
        Expired,
        Cancelled
    }

    public class Subscription
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Pending;
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? PaymobOrderId { get; set; }
        public string? PaymobTransactionId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Tenant? Tenant { get; set; }
    }
}