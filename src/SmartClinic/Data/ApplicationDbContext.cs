using Microsoft.EntityFrameworkCore;

namespace SmartClinic.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Add DbSet properties here for your entities, for example:
        // public DbSet<Patient> Patients { get; set; } = null!;
    }
}
