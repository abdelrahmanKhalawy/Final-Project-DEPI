using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SehhaTech.Infrastructure.Data;

namespace SehhaTech.API.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext db)
        {
            // لو مش متسجل دخول، عدي
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await _next(context);
                return;
            }

            // لو SuperAdmin، عدي من غير ما تتحقق من التيننت
            var role = context.User.FindFirst(ClaimTypes.Role)?.Value;
            if (role == "SuperAdmin")
            {
                await _next(context);
                return;
            }

            // عدي لو الـ request للـ subscription initiate عشان الكلينك الجديد يقدر يدفع
            if (context.Request.Path.StartsWithSegments("/api/subscription/initiate"))
            {
                await _next(context);
                return;
            }

            // جيب الـ TenantId من الـ Token
            var tenantIdClaim = context.User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantIdClaim) || !int.TryParse(tenantIdClaim, out int tenantId))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Invalid tenant" });
                return;
            }

            // تأكد إن العيادة Active
            var tenant = await db.Tenants
                .FirstOrDefaultAsync(t => t.Id == tenantId && t.IsActive);

            if (tenant == null)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new { message = "Clinic subscription is inactive" });
                return;
            }

            // حط الـ TenantId في الـ HttpContext عشان أي Controller يقدر يوصله
            context.Items["TenantId"] = tenantId;

            await _next(context);
        }
    }
}