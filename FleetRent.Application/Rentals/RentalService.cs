using FleetRent.Application.Rentals.Dtos;
using FleetRent.Domain.Entities;
using FleetRent.Domain.Exceptions;
using FleetRent.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FleetRent.Application.Rentals
{
    public class RentalService(IRentalRepository rentalRepo, IDriverRepository driverRepo, IBikeRepository bikeRepo, IUnitOfWork uow, ILogger<RentalService> logger) : IRentalService
    {
        private readonly IRentalRepository _rentalRepo = rentalRepo;
        private readonly IDriverRepository _driverRepo = driverRepo;
        private readonly IBikeRepository _bikeRepo = bikeRepo;
        private readonly IUnitOfWork _uow = uow;
        private readonly ILogger<RentalService> _logger = logger;

        public async Task<RentalDto> CreateAsync(CreateRentalDto dto, CancellationToken ct = default)
        {
            // Validate the driver existence and license requirements.
            _logger.LogInformation("Attempting to create rental for driver {DriverId} with bike {BikeId} starting {StartDate} on plan {PlanType}.", dto.DriverId, dto.BikeId, dto.StartDate, dto.PlanType);
            var driver = await _driverRepo.GetByIdAsync(dto.DriverId, ct)
                ?? throw DomainException.From(DomainErrors.Driver.NotFound);

            if (!driver.IsCategoryAEnabled())
            {
                _logger.LogWarning("Driver {DriverId} is not eligible for rentals because license category A is not enabled.", dto.DriverId);
                throw DomainException.From(DomainErrors.Driver.NotCategoryA);
            }

            // Validate the requested bike and ensure it exists in the catalog.
            var bike = await _bikeRepo.GetByIdAsync(dto.BikeId, ct)
                ?? throw DomainException.From(DomainErrors.Bike.NotFound);

            // Prevent rentals from starting in the past so pricing calculations remain predictable.
            if (dto.StartDate < DateTime.Now)
            {
                _logger.LogWarning("Rental creation rejected because requested start date {StartDate} is in the past.", dto.StartDate);
                throw DomainException.From(DomainErrors.Rental.StartDateInThePast);
            }

            // Generate a sequential human-friendly identifier for traceability.
            var existingRentalsCount = await _rentalRepo.CountAsync(ct);
            var identifier = $"RENT-{existingRentalsCount + 1:0000}";

            // Instantiate the aggregate with the computed identifier and plan definition.
            var rental = new Rental(driver.Id, bike.Id, identifier, dto.StartDate, dto.PlanType);

            // Ensure there is no overlapping rental for the same bike during the requested period.
            var bikeBusy = await _rentalRepo.HasActiveRentalForBikeAsync(
                bike.Id, rental.StartDate, rental.PlannedEndDate, ct);

            if (bikeBusy)
            {
                _logger.LogWarning("Rental creation rejected because bike {BikeId} is already rented during the requested period {Start} - {End}.", bike.Id, rental.StartDate, rental.PlannedEndDate);
                throw DomainException.From(DomainErrors.Rental.BikeAlreadyRented);
            }

            await _rentalRepo.AddAsync(rental, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Rental {RentalId} created successfully with identifier {Identifier}.", rental.Id, rental.Identifier);

            return RentalDto.FromEntity(rental);
        }

        public async Task<RentalDto> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving rental {RentalId}.", id);
            var rental = await _rentalRepo.GetByIdAsync(id, ct);
            if (rental is null)
            {
                _logger.LogWarning("Rental {RentalId} not found.", id);
            }
            else
            {
                _logger.LogInformation("Rental {RentalId} retrieved successfully.", id);
            }
            return rental is null ? null : RentalDto.FromEntity(rental);
        }

        public async Task<RentalDto> CloseAsync(Guid id, CloseRentalDto dto, CancellationToken ct = default)
        {
            _logger.LogInformation("Attempting to close rental {RentalId} at {ReturnDate}.", id, dto.ReturnDate);
            var rental = await _rentalRepo.GetByIdAsync(id, ct);
            if (rental is null) return null;

            var total = rental.CloseAndCalculateTotal(dto.ReturnDate);

            await _uow.SaveChangesAsync(ct);

            var result = RentalDto.FromEntity(rental);
            result.TotalCost = total;
            result.ReturnDate = dto.ReturnDate;

            _logger.LogInformation("Rental {RentalId} closed successfully with total {TotalCost}.", id, total);
            return result;
        }
    }
}
