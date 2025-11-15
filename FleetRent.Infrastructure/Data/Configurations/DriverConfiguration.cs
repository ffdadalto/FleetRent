using FleetRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetRent.Infrastructure.Data.Configurations
{
    public class DriverConfiguration : IEntityTypeConfiguration<Driver>
    {
        public void Configure(EntityTypeBuilder<Driver> builder)
        {
            builder.ToTable("Drivers");
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Identifier).IsRequired().HasMaxLength(50);
            builder.Property(d => d.Name).IsRequired().HasMaxLength(200);
            builder.Property(d => d.Cnpj).IsRequired().HasMaxLength(14);
            builder.Property(d => d.LicenseNumber).IsRequired().HasMaxLength(20);
            builder.Property(d => d.LicenseType)
                .HasConversion<string>()
                .IsRequired();

            builder.HasIndex(d => d.Cnpj).IsUnique();
            builder.HasIndex(d => d.LicenseNumber).IsUnique();
        }
    }
}
