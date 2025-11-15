using FleetRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetRent.Infrastructure.Data.Configurations
{
    public class BikeConfiguration : IEntityTypeConfiguration<Bike>
    {
        public void Configure(EntityTypeBuilder<Bike> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Model).IsRequired().HasMaxLength(100);
            builder.Property(m => m.Plate).IsRequired().HasMaxLength(10);

            builder.HasIndex(m => m.Plate).IsUnique();

            builder.HasMany(m => m.Rentals)
                   .WithOne(r => r.Bike)
                   .HasForeignKey(r => r.BikeId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
