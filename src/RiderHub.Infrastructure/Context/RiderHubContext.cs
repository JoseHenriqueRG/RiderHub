using Microsoft.EntityFrameworkCore;
using RiderHub.Domain.Entities;

namespace RiderHub.Infrastructure.Context
{
    public class RiderHubContext : DbContext
    {
        public RiderHubContext(DbContextOptions<RiderHubContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DeliveryDriver>()
                .Property(d => d.DriverLicenseType)
                .HasConversion<string>();
        }

        public DbSet<Motorcycle> Motorcycles { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<DeliveryDriver> DeliveryDrivers { get; set; }
        public DbSet<RentalPlan> RentalPlans { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    }
}
