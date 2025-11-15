using FleetRent.Application.Drivers.Dtos;

namespace FleetRent.Application.Drivers
{
    public interface IDriverService
    {
        Task<DriverDto> CreateAsync(CreateDriverDto dto, CancellationToken ct = default);
        Task<DriverDto> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<DriverDto>> GetAllAsync();

        Task<DriverDto> UpdateLicenseImageAsync(Guid driverId, Stream imageStream, string fileName, string contentType, CancellationToken ct = default);
    }
}
