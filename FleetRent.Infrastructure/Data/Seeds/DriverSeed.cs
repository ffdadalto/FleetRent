using FleetRent.Domain.Entities;
using FleetRent.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FleetRent.Infrastructure.Data.Seeds
{
    public static class DriverSeed
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Driver>().HasData(
                new
                {
                    Id = Guid.Parse("ee813936-8f43-4a96-a05e-f0b3f5e0a876"),
                    Identifier = "JS1990",
                    Name = "João da Silva",
                    Cnpj = "11222333000144",
                    BirthDate = new DateOnly(1990, 5, 15),
                    LicenseNumber = "01234567890",
                    LicenseType = LicenseType.A,
                    LicenseImagePath = (string)null
                },
                new
                {
                    Id = Guid.Parse("4f5d5e1a-f761-4c60-8f9f-6a7e0a8d6b9c"),
                    Identifier = "MO1985",
                    Name = "Maria Oliveira",
                    Cnpj = "5552333000144",
                    BirthDate = new DateOnly(1985, 10, 2),
                    LicenseNumber = "09876543211",
                    LicenseType = LicenseType.B,
                    LicenseImagePath = (string)null
                },
                new
                {
                    Id = Guid.Parse("b1a6a3a1-7c9c-4f8e-8a0b-9d6c1b3f5e2d"),
                    Identifier = "CP1978",
                    Name = "Carlos Pereira",
                    Cnpj = "5552845000144",
                    BirthDate = new DateOnly(1978, 1, 30),
                    LicenseNumber = "05566778899",
                    LicenseType = LicenseType.AB,
                    LicenseImagePath = (string)null
                }
            );
        }
    }
}

