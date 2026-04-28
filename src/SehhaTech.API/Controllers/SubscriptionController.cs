using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SehhaTech.Core.Interfaces;
using SehhaTech.Core.Models;
using SehhaTech.Infrastructure.Data;
using System.Security.Claims;
using System.Text.Json;

namespace SehhaTech.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionController : ControllerBase
    {
        private readonly IPaymobService _paymobService;
        private readonly AppDbContext _context;

        public SubscriptionController(IPaymobService paymobService, AppDbContext context)
        {
            _paymobService = paymobService;
            _context = context;
        }

        // Step 1: الكلينك يطلب الدفع
        // POST api/subscription/initiate/{tenantId}
        [HttpPost("initiate/{tenantId}")]
        public async Task<IActionResult> InitiatePayment(int tenantId)
        {
            try
            {
                var tenant = await _context.Tenants.FindAsync(tenantId);
                if (tenant == null)
                    return NotFound(new { message = "Clinic not found" });

                decimal amount = 500; // سعر الاشتراك ثابت

                // Paymob Steps
                var authToken = await _paymobService.GetAuthTokenAsync();
                var orderId = await _paymobService.CreateOrderAsync(authToken, amount);
                var paymentKey = await _paymobService.GetPaymentKeyAsync(authToken, orderId, amount, tenantId);
                var iframeUrl = await _paymobService.GetIframeUrlAsync(paymentKey);

                // حفظ الـ Subscription كـ Pending
                var subscription = new Subscription
                {
                    TenantId = tenantId,
                    Status = SubscriptionStatus.Pending,
                    Amount = amount,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(1),
                    PaymobOrderId = orderId.ToString()
                };

                _context.Subscriptions.Add(subscription);
                await _context.SaveChangesAsync();

                return Ok(new { iframeUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Step 2: Paymob Callback بعد الدفع
        // POST api/subscription/callback
        [HttpPost("callback")]
        public async Task<IActionResult> PaymobCallback([FromBody] JsonElement body)
        {
            try
            {
                var obj = body.GetProperty("obj");
                var success = obj.GetProperty("success").GetBoolean();
                var orderId = obj.GetProperty("order").GetProperty("id").GetInt32().ToString();

                if (!success)
                    return Ok(new { message = "Payment failed" });

                // جيب الـ Subscription بالـ OrderId
                var subscription = await _context.Subscriptions
                    .Include(s => s.Tenant)
                    .FirstOrDefaultAsync(s => s.PaymobOrderId == orderId);

                if (subscription == null)
                    return NotFound(new { message = "Subscription not found" });

                // فعل الاشتراك والعيادة
                subscription.Status = SubscriptionStatus.Active;
                subscription.Tenant!.IsActive = true;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Payment successful" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}