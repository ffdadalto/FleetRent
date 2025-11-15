using FleetRent.Domain.Entities;
using FleetRent.Domain.Exceptions;
using Xunit;

namespace FleetRent.Tests.Domain;

public class BikeTests
{
    [Fact]
    public void ChangePlate_ShouldNormalizePlate()
    {
        // Arrange
        var bike = new Bike("Bike001", 2024, "Model X", "ABC1D23");

        // Act
        bike.ChangePlate(" xyz9p87 ");

        // Assert
        Assert.Equal("XYZ9P87", bike.Plate);
    }

    [Fact]
    public void ChangePlate_ShouldThrow_WhenPlateIsEmpty()
    {
        // Arrange
        var bike = new Bike("Bike001", 2024, "Model X", "ABC1D23");

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => bike.ChangePlate("   "));
        Assert.Equal(DomainErrors.Bike.PlateRequired.Message, exception.Message);
    }
}
