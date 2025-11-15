using FleetRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetRent.Infrastructure.Data.Configurations
{
    public class RentalConfiguration : IEntityTypeConfiguration<Rental>
    {
        public void Configure(EntityTypeBuilder<Rental> builder)
        {
            builder.ToTable("Rentals");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.StartDate).HasColumnType("date");
            builder.Property(r => r.PlannedEndDate).HasColumnType("date");
            builder.Property(r => r.EndDate).HasColumnType("date");

            builder.HasOne(r => r.Bike)
                .WithMany(b => b.Rentals)
                .HasForeignKey(r => r.BikeId);

            builder.HasOne(r => r.Driver)
                .WithMany(d => d.Rentals)
                .HasForeignKey(r => r.DriverId);

            builder.OwnsOne(r => r.Plan, owned =>
            {
                owned.Property(p => p.Type)
                    .HasConversion<string>()
                    .HasColumnName("PlanType")
                    .IsRequired();

                owned.Property(p => p.Days)
                    .HasColumnName("PlanDays")
                    .IsRequired();

                owned.Property(p => p.DailyRate)
                    .HasColumnName("PlanDailyRate")
                    .HasColumnType("numeric(10,2)")
                    .IsRequired();

                owned.Property(p => p.EarlyReturnFinePercentage)
                    .HasColumnName("PlanEarlyReturnFine")
                    .HasColumnType("numeric(5,4)")
                    .IsRequired();
            });
        }
    }
}
