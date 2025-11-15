using FleetRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FleetRent.Infrastructure.Data.Seeds
{
    public static class BikeSeed
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bike>().HasData(
                new Bike("Bike001", 2021, "Bike Sport", "ABC1D23"),
                new Bike("Bike002", 2022, "Bike E", "ABC1C34"),
                new Bike("Bike003", 2023, "Bike Pop", "XYZ1C38")                
            );
        }
    }
}
