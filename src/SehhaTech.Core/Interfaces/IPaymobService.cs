namespace SehhaTech.Core.Interfaces
{
    public interface IPaymobService
    {
        Task<string> GetAuthTokenAsync();
        Task<int> CreateOrderAsync(string authToken, decimal amount);
        Task<string> GetPaymentKeyAsync(string authToken, int orderId, decimal amount, int tenantId);
        Task<string> GetIframeUrlAsync(string paymentKey);
    }
}