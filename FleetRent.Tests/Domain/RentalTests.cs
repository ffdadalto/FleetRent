using FleetRent.Domain.Entities;
using FleetRent.Domain.Enums;
using FleetRent.Domain.Exceptions;
using Xunit;

namespace FleetRent.Tests.Domain;

public class RentalTests
{
    [Fact]
    public void CloseAndCalculateTotal_ShouldReturnBaseCost_WhenReturnedOnPlannedEndDate()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        var bikeId = Guid.NewGuid();
        var startDate = new DateTime(2024, 5, 1);
        var rental = new Rental(driverId, bikeId, "RENT-0001", new DateTime(2024, 5, 1), RentalPlanType.Days7);
        var plannedEndDate = rental.PlannedEndDate;

        // Act
        var total = rental.CloseAndCalculateTotal(plannedEndDate);

        // Assert
        Assert.Equal(rental.Plan.Days * rental.Plan.DailyRate, total);
        Assert.Equal(plannedEndDate, rental.EndDate);
    }   

    [Fact]
    public void CloseAndCalculateTotal_ShouldAddLateFee_WhenReturningAfterPlannedEndDate()
    {
        // Arrange
        var rental = new Rental(Guid.NewGuid(), Guid.NewGuid(), "RENT-0003", new DateTime(2024, 7, 1), RentalPlanType.Days7);
        var lateReturnDate = rental.PlannedEndDate.AddDays(2);

        // Act
        var total = rental.CloseAndCalculateTotal(lateReturnDate);

        // Assert
        var expected = (rental.Plan.Days * rental.Plan.DailyRate) + (2 * 50m);
        Assert.Equal(expected, total);
    }

    [Fact]
    public void CloseAndCalculateTotal_ShouldThrow_WhenReturningBeforeStartDate()
    {
        // Arrange
        var rental = new Rental(Guid.NewGuid(), Guid.NewGuid(), "RENT-0004", new DateTime(2024, 8, 1), RentalPlanType.Days7);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => rental.CloseAndCalculateTotal(rental.StartDate.AddDays(-1)));
        Assert.Equal(DomainErrors.Rental.ReturnBeforeStart.Message, exception.Message);
    }

}
