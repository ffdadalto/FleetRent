using FleetRent.Domain.Entities;
using FleetRent.Domain.Enums;
using Xunit;

namespace FleetRent.Tests.Domain;

public class DriverTests
{
    [Theory]
    [InlineData(LicenseType.A, true)]
    [InlineData(LicenseType.AB, true)]
    [InlineData(LicenseType.B, false)]
    public void IsCategoryAEnabled_ShouldReflectLicenseType(LicenseType licenseType, bool expected)
    {
        // Arrange
        var driver = new Driver("DRV001", "John Doe", "12345678000101", new DateOnly(1990, 1, 1), "LIC123456", licenseType);

        // Act
        var result = driver.IsCategoryAEnabled();

        // Assert
        Assert.Equal(expected, result);
    }
}
