using Microsoft.EntityFrameworkCore;
using SehhaTech.Core.Models;

namespace SehhaTech.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Tables
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tenant
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Name).IsRequired().HasMaxLength(200);
                entity.Property(t => t.Email).IsRequired().HasMaxLength(200);
                entity.HasIndex(t => t.Email).IsUnique();
            });

            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Role).HasConversion<string>();

                entity.HasOne(u => u.Tenant)
                      .WithMany(t => t.Users)
                      .HasForeignKey(u => u.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Subscription
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Amount).HasColumnType("decimal(18,2)");
                entity.Property(s => s.Status).HasConversion<string>();

                entity.HasOne(s => s.Tenant)
                      .WithOne(t => t.Subscription)
                      .HasForeignKey<Subscription>(s => s.TenantId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Patient
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.FullName).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Phone).IsRequired().HasMaxLength(20);

                entity.HasOne(p => p.Tenant)
                      .WithMany()
                      .HasForeignKey(p => p.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Doctor
            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasKey(d => d.Id);

                entity.HasOne(d => d.Tenant)
                      .WithMany()
                      .HasForeignKey(d => d.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.User)
                      .WithMany()
                      .HasForeignKey(d => d.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Appointment
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Status).HasConversion<string>();

                entity.HasOne(a => a.Tenant)
                      .WithMany()
                      .HasForeignKey(a => a.TenantId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Patient)
                      .WithMany(p => p.Appointments)
                      .HasForeignKey(a => a.PatientId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Doctor)
                      .WithMany(d => d.Appointments)
                      .HasForeignKey(a => a.DoctorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

    }

}