using FleetRent.Application.Rentals;
using FleetRent.Application.Rentals.Dtos;
using FleetRent.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FleetRent.Api.Controllers
{
    [ApiController]
    [Route("rentals")]
    public class RentalsController(IRentalService rentalService, ILogger<RentalsController> logger) : ControllerBase
    {
        private readonly IRentalService _rentalService = rentalService;
        private readonly ILogger<RentalsController> _logger = logger;

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(DomainError), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateRentalDto dto, CancellationToken ct)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for creating rental: {ModelState}", ModelState);
                    throw new DomainException(DomainErrors.Rental.InvalidData);
                }

                _logger.LogInformation("Received request to create rental for driver {DriverId} and bike {BikeId} starting {StartDate}.", dto.DriverId, dto.BikeId, dto.StartDate);
                var rental = await _rentalService.CreateAsync(dto, ct);
                _logger.LogInformation("Rental {RentalId} created successfully with identifier {Identifier}.", rental.Id, rental.Identifier);
                return StatusCode(StatusCodes.Status201Created);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Failed to create rental due to domain validation error {Message}.", ex.Error.Message);
                if (ex.Error == DomainErrors.Rental.NotFound || ex.Error == DomainErrors.Rental.NotFound)
                    return BadRequest(new { message = ex.Error.Message });

                return BadRequest(new { message = ex.Error.Message });
            }
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DomainError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DomainError), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RentalDto>> GetById(Guid id, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Fetching rental {RentalId}.", id);
                var rental = await _rentalService.GetByIdAsync(id, ct);
                if (rental is null) throw new DomainException(DomainErrors.Rental.NotFound);
                _logger.LogInformation("Rental {RentalId} retrieved successfully.", id);
                return Ok(rental);
            }
            catch (DomainException ex) when (ex.Error == DomainErrors.Rental.NotFound)
            {
                _logger.LogWarning(ex, "Rental {RentalId} was not found.", id);
                return BadRequest(new { message = ex.Error.Message });
            }
            catch (DomainException ex) when (ex.Error == DomainErrors.Rental.BadRequest)
            {
                _logger.LogWarning(ex, "Failed to retrieve information from Rental {RentalId}.Erro: {Message}", id, ex.Error.Message);
                return BadRequest(new { message = ex.Error.Message });
            }
        }

        [HttpPut("{id:guid}/return")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DomainError), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RentalDto>> Close(Guid id, [FromBody] CloseRentalDto dto, CancellationToken ct)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for updating rental: {ModelState}", ModelState);
                    throw new DomainException(DomainErrors.Rental.InvalidData);
                }

                _logger.LogInformation("Request to close rental {RentalId} at {ReturnDate}.", id, dto.ReturnDate);
                var updated = await _rentalService.CloseAsync(id, dto, ct);
                if (updated is null) throw new DomainException(DomainErrors.Rental.NotFound);
                _logger.LogInformation("Rental {RentalId} closed successfully with total {TotalCost}.", id, updated.TotalCost);
                return Ok(updated);
            }
            catch (DomainException ex) when (ex.Error == DomainErrors.Rental.NotFound)
            {
                _logger.LogWarning(ex, "Failed to close rental {RentalId} because it was not found.", id);
                return BadRequest(new { message = ex.Error.Message });
            }
        }
    }
}
