using FleetRent.Application.Bikes.Dtos;
using FleetRent.Domain.Entities;
using FleetRent.Domain.Exceptions;
using FleetRent.Domain.Interfaces;
using FleetRent.Infrastructure.Messaging;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FleetRent.Application.Bikes
{ 
    public class BikeService(IBikeRepository bikeRepo, IUnitOfWork uow, IRabbitMqPublisher publisher, ILogger<BikeService> logger) : IBikeService
    {
        private readonly IBikeRepository _bikeRepo = bikeRepo;
        private readonly IUnitOfWork _uow = uow;
        private readonly IRabbitMqPublisher _publisher = publisher;
        private readonly ILogger<BikeService> _logger = logger;

        public async Task<BikeDto> CreateAsync(CreateBikeDto dto)
        {
            _logger.LogInformation("Attempting to create bike with identifier {Identifier} and plate {Plate}.", dto.Identifier, dto.Plate);
            var existing = await _bikeRepo.GetByPlateAsync(dto.Plate);
            if (existing is not null)
            {
                _logger.LogWarning("Bike creation rejected because plate {Plate} already exists (BikeId: {ExistingBikeId}).", dto.Plate, existing.Id);
                throw DomainException.From(DomainErrors.Bike.PlateAlreadyExists);
            }

            var bike = new Bike(dto.Identifier, dto.Year, dto.Model, dto.Plate);
            await _bikeRepo.AddAsync(bike);
            await _uow.SaveChangesAsync();

            var message = JsonSerializer.Serialize(new BikeCreatedMessage(bike.Id, bike.Identifier, bike.Year));
            await _publisher.PublishAsync("bike.created", message);

            _logger.LogInformation("Bike {BikeId} created successfully and message published with routing key {RoutingKey}.", bike.Id, "bike.created");

            return BikeDto.FromEntity(bike);
        }

        public async Task<IReadOnlyCollection<BikeDto>> GetAsync(string? plate)
        {
            _logger.LogInformation("Retrieving bikes filtered by plate {Plate}.", plate);
            var bikes = await _bikeRepo.GetAsync(plate);
            _logger.LogInformation("Retrieved {Count} bike(s) for filter {Plate}.", bikes.Count, plate);
            return [.. bikes.Select(BikeDto.FromEntity)];
        }

        public async Task<BikeDto> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Retrieving bike {BikeId}.", id);
            var bike = await _bikeRepo.GetByIdAsync(id);
            if (bike is null)
            {
                _logger.LogWarning("Bike {BikeId} not found.", id);
            }
            else
            {
                _logger.LogInformation("Bike {BikeId} retrieved successfully.", id);
            }
            return bike is null ? null : BikeDto.FromEntity(bike);
        }

        public async Task<BikeDto> UpdatePlateAsync(Guid id, UpdateBikePlateDto dto)
        {
            _logger.LogInformation("Attempting to update bike {BikeId} plate to {Plate}.", id, dto.Plate);
            var bike = await _bikeRepo.GetByIdAsync(id);
            if (bike is null) return null;

            if (!string.Equals(bike.Plate, dto.Plate, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _bikeRepo.GetByPlateAsync(dto.Plate);
                if (existing is not null)
                {
                    _logger.LogWarning("Bike {BikeId} plate update rejected because target plate {Plate} is already used by bike {ExistingBikeId}.", id, dto.Plate, existing.Id);
                    throw DomainException.From(DomainErrors.Bike.PlateAlreadyExists);
                }
            }

            bike.ChangePlate(dto.Plate);
            _bikeRepo.Update(bike);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Bike {BikeId} plate updated to {Plate}.", id, bike.Plate);

            return BikeDto.FromEntity(bike);
        }

        public async Task DeleteAsync(Guid id)
        {
            _logger.LogInformation("Attempting to delete bike {BikeId}.", id);
            var bike = await _bikeRepo.GetByIdAsync(id);
            if (bike is null)
            {
                _logger.LogWarning("Bike {BikeId} delete rejected because it does not exist.", id);
                throw DomainException.From(DomainErrors.Bike.NotFound);
            }

            if (!bike.CanDelete)
            {
                _logger.LogWarning("Bike {BikeId} cannot be deleted because there are active rentals.", id);
                throw DomainException.From(DomainErrors.Bike.CannotRemoveWithRentals);
            }

            _bikeRepo.Remove(bike);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Bike {BikeId} deleted successfully.", id);
        }
    }
}
