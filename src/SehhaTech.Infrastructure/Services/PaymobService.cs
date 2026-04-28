using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SehhaTech.Core.Interfaces;

namespace SehhaTech.Infrastructure.Services
{
    public class PaymobService : IPaymobService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private readonly string _integrationId;
        private readonly string _iframeId;

        public PaymobService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _apiKey = _configuration["PaymobSettings:ApiKey"]!;
            _integrationId = _configuration["PaymobSettings:IntegrationId"]!;
            _iframeId = _configuration["PaymobSettings:IframeId"]!;
        }

        // Step 1: Get Auth Token
        public async Task<string> GetAuthTokenAsync()
        {
            var payload = new { api_key = _apiKey };
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(
                "https://accept.paymob.com/api/auth/tokens",
                content
            );

            var responseString = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(responseString);
            return json.RootElement.GetProperty("token").GetString()!;
        }

        // Step 2: Create Order
        public async Task<int> CreateOrderAsync(string authToken, decimal amount)
        {
            var payload = new
            {
                auth_token = authToken,
                delivery_needed = false,
                amount_cents = (int)(amount * 100),
                currency = "EGP",
                items = new object[] { }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(
                "https://accept.paymob.com/api/ecommerce/orders",
                content
            );

            var responseString = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(responseString);
            return json.RootElement.GetProperty("id").GetInt32();
        }

        // Step 3: Get Payment Key
        public async Task<string> GetPaymentKeyAsync(string authToken, int orderId, decimal amount, int tenantId)
        {
            var payload = new
            {
                auth_token = authToken,
                amount_cents = (int)(amount * 100),
                expiration = 3600,
                order_id = orderId,
                billing_data = new
                {
                    apartment = "NA",
                    email = "clinic@sehhatech.com",
                    floor = "NA",
                    first_name = "Clinic",
                    street = "NA",
                    building = "NA",
                    phone_number = "01000000000",
                    shipping_method = "NA",
                    postal_code = "NA",
                    city = "NA",
                    country = "EG",
                    last_name = "Owner",
                    state = "NA"
                },
                currency = "EGP",
                integration_id = int.Parse(_integrationId),
                lock_order_when_paid = false
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(
                "https://accept.paymob.com/api/acceptance/payment_keys",
                content
            );

            var responseString = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(responseString);
            return json.RootElement.GetProperty("token").GetString()!;
        }

        // Step 4: Get Iframe URL
        public async Task<string> GetIframeUrlAsync(string paymentKey)
        {
            return $"https://accept.paymob.com/api/acceptance/iframes/{_iframeId}?payment_token={paymentKey}";
        }
    }
}