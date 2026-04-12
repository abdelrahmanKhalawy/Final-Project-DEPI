using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("SQLSERVER_CONNECTION_STRING");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("A SQL Server connection string is required. Configure ConnectionStrings:DefaultConnection in appsettings.json or set SQLSERVER_CONNECTION_STRING.");
}

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<SmartClinic.Data.ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SmartClinic.Data.ApplicationDbContext>();
    try
    {
        if (!context.Database.CanConnect())
        {
            throw new InvalidOperationException("Database connection failed. Verify the SQL Server instance and the connection string in appsettings.json.");
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Unable to connect to the database.");
        throw;
    }
}

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
