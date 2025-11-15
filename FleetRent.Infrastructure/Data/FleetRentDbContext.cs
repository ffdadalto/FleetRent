using FleetRent.Domain.Entities;
using FleetRent.Domain.Interfaces;
using FleetRent.Infrastructure.Data.Seeds;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FleetRent.Infrastructure.Data
{
    public class FleetRentDbContext(DbContextOptions<FleetRentDbContext> options) : DbContext(options), IUnitOfWork
    {
        public DbSet<Bike> Bikes => Set<Bike>();
        public DbSet<Driver> Drivers => Set<Driver>();
        public DbSet<Rental> Rentals => Set<Rental>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {            
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            BikeSeed.Seed(modelBuilder);
            DriverSeed.Seed(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }   

        public override Task<int> SaveChangesAsync(CancellationToken ct = default)
            => base.SaveChangesAsync(ct);
    }
}
