using FleetRent.Application.Rentals;
using FleetRent.Application.Rentals.Dtos;
using FleetRent.Domain.Entities;
using FleetRent.Domain.Enums;
using FleetRent.Domain.Exceptions;
using FleetRent.Infrastructure.Data;
using FleetRent.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FleetRent.Tests.Application;

public class RentalServiceTests
{
    private static FleetRentDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<FleetRentDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        var context = new FleetRentDbContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistRental_WhenDriverAndBikeAreValid()
    {
        // Arrange
        await using var context = CreateContext(Guid.NewGuid().ToString());
        var driver = new Driver("DRV001", "Alice Rider", "12345678000101", new DateOnly(1992, 3, 10), "LIC123", LicenseType.AB);
        var bike = new Bike("Bike001", 2023, "e-Bike", "ABC1D23");
        await context.Drivers.AddAsync(driver);
        await context.Bikes.AddAsync(bike);
        await context.SaveChangesAsync();

        var rentalRepo = new RentalRepository(context);
        var driverRepo = new DriverRepository(context);
        var bikeRepo = new BikeRepository(context);
        var service = new RentalService(rentalRepo, driverRepo, bikeRepo, context, NullLogger<RentalService>.Instance);

        var startDate = DateTime.UtcNow.AddDays(2);
        var dto = new CreateRentalDto
        {
            DriverId = driver.Id,
            BikeId = bike.Id,
            StartDate = startDate,
            PlanType = RentalPlanType.Days7
        };

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("RENT-0001", result.Identifier);
        Assert.Equal(driver.Id, result.DriverId);
        Assert.Equal(bike.Id, result.BikeId);
        Assert.Equal(RentalPlanType.Days7, result.PlanType);

        var persisted = await context.Rentals.FirstOrDefaultAsync();
        Assert.NotNull(persisted);
        Assert.Equal(DateOnly.FromDateTime(startDate.AddDays(1)), DateOnly.FromDateTime(persisted!.StartDate));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenBikeAlreadyRentedInPeriod()
    {
        // Arrange
        await using var context = CreateContext(Guid.NewGuid().ToString());
        var driver = new Driver("DRV001", "Alice Rider", "12345678000101", new DateOnly(1992, 3, 10), "LIC123", LicenseType.AB);
        var anotherDriver = new Driver("DRV002", "Bob Driver", "98765432000199", new DateOnly(1990, 8, 15), "LIC999", LicenseType.A);
        var bike = new Bike("Bike001", 2023, "e-Bike", "ABC1D23");
        var existingRental = new Rental(driver.Id, bike.Id, "RENT-0001", DateTime.UtcNow.AddDays(4), RentalPlanType.Days7);
        await context.AddRangeAsync(driver, anotherDriver, bike, existingRental);
        await context.SaveChangesAsync();

        var rentalRepo = new RentalRepository(context);
        var driverRepo = new DriverRepository(context);
        var bikeRepo = new BikeRepository(context);
        var service = new RentalService(rentalRepo, driverRepo, bikeRepo, context, NullLogger<RentalService>.Instance);

        var dto = new CreateRentalDto
        {
            DriverId = anotherDriver.Id,
            BikeId = bike.Id,
            StartDate = DateTime.UtcNow.AddDays(5),
            PlanType = RentalPlanType.Days7
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => service.CreateAsync(dto));
        Assert.Equal(DomainErrors.Rental.BikeAlreadyRented.Message, exception.Message);
    }
}
