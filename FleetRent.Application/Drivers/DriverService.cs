using FleetRent.Application.Drivers.Dtos;
using FleetRent.Domain.Entities;
using FleetRent.Domain.Exceptions;
using FleetRent.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FleetRent.Application.Drivers
{
    public class DriverService(IDriverRepository driverRepo, IUnitOfWork uow, IFileStorageService fileStorageService, ILogger<DriverService> logger) : IDriverService
    {
        private readonly IDriverRepository _driverRepo = driverRepo;
        private readonly IUnitOfWork _uow = uow;
        private readonly IFileStorageService _fileStorageService = fileStorageService;
        private readonly ILogger<DriverService> _logger = logger;

        public async Task<DriverDto> CreateAsync(CreateDriverDto dto, CancellationToken ct = default)
        {
            // Guard: identifiers must be unique to keep the entity consistent.
            _logger.LogInformation("Attempting to create driver with identifier {Identifier} and license {LicenseNumber}.", dto.Identifier, dto.LicenseNumber);
            if (await _driverRepo.GetByCnpjAsync(dto.Cnpj, ct) is not null)
            {
                _logger.LogWarning("Driver creation rejected because CNPJ {Cnpj} already exists.", dto.Cnpj);
                throw DomainException.From(DomainErrors.Driver.CnpjAlreadyExists);
            }

            if (await _driverRepo.GetByLicenseNumberAsync(dto.LicenseNumber, ct) is not null)
            {
                _logger.LogWarning("Driver creation rejected because license number {LicenseNumber} already exists.", dto.LicenseNumber);
                throw DomainException.From(DomainErrors.Driver.LicenseNumberAlreadyExists);
            }

            var driver = new Driver(
                identifier: dto.Identifier,
                name: dto.Name,
                cnpj: dto.Cnpj,
                birthDate: dto.BirthDate,
                licenseNumber: dto.LicenseNumber,
                licenseType: dto.LicenseType
            );

            await _driverRepo.AddAsync(driver, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Driver {DriverId} created successfully with identifier {Identifier}.", driver.Id, driver.Identifier);

            return DriverDto.FromEntity(driver);
        }

        public async Task<IEnumerable<DriverDto>> GetAllAsync()
        {
            _logger.LogInformation("Retrieving all drivers.");
            var driver = await _driverRepo.GetAllAsync();
            _logger.LogInformation("Retrieved {Count} driver(s).", driver.Count());
            return driver.Select(DriverDto.FromEntity);
        }

        public async Task<DriverDto> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving driver {DriverId}.", id);
            var driver = await _driverRepo.GetByIdAsync(id, ct);
            if (driver is null)
            {
                _logger.LogWarning("Driver {DriverId} not found.", id);
            }
            else
            {
                _logger.LogInformation("Driver {DriverId} retrieved successfully.", id);
            }
            return driver is null ? null : DriverDto.FromEntity(driver);
        }

        public async Task<DriverDto> UpdateLicenseImageAsync(Guid driverId, Stream imageStream, string fileName, string contentType, CancellationToken ct = default)
        {
            _logger.LogInformation("Attempting to update license image for driver {DriverId} with file {FileName}.", driverId, fileName);
            var driver = await _driverRepo.GetByIdAsync(driverId, ct);
            if (driver is null) return null;

            // Persist the uploaded file and store the relative path inside the aggregate.
            var relativePath = await _fileStorageService.UploadFileAsync(imageStream, fileName, contentType);

            driver.UpdateLicenseImage(relativePath);
            _driverRepo.Update(driver);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Driver {DriverId} license image updated successfully to {RelativePath}.", driverId, relativePath);

            return DriverDto.FromEntity(driver);
        }

    }
}
