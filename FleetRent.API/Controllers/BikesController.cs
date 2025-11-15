using FleetRent.Application.Bikes;
using FleetRent.Application.Bikes.Dtos;
using FleetRent.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FleetRent.Api.Controllers
{
    [ApiController]
    [Route("bikes")]
    public class BikesController(IBikeService bikeService, ILogger<BikesController> logger) : ControllerBase
    {
        private readonly IBikeService _bikeService = bikeService;
        private readonly ILogger<BikesController> _logger = logger;

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(DomainError), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateBikeDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for creating bike: {ModelState}", ModelState);
                    throw new DomainException(DomainErrors.Bike.InvalidData);
                }

                _logger.LogInformation("Received request to create bike with identifier {Identifier} and plate {Plate}.", dto.Identifier, dto.Plate);
                var bike = await _bikeService.CreateAsync(dto);
                _logger.LogInformation("Bike {BikeId} created successfully with plate {Plate}.", bike.Id, bike.Plate);
                return StatusCode(StatusCodes.Status201Created, "");
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Failed to create bike due to domain validation error {Message}.", ex.Error.Message);
                return BadRequest(new { message = ex.Error.Message });
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BikeDto>>> Get([FromQuery] string plate)
        {
            _logger.LogInformation("Listing bikes filtered by plate {Plate}.", plate);
            var bikes = await _bikeService.GetAsync(plate);
            _logger.LogInformation("Returned {Count} bike(s) for filter {Plate}.", bikes.Count, plate);
            return Ok(bikes);
        }

        [HttpPut("{id:guid}/plate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DomainError), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdatePlate(Guid id, [FromBody] UpdateBikePlateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for updating bike: {ModelState}", ModelState);
                    throw new DomainException(DomainErrors.Bike.InvalidData);
                }

                _logger.LogInformation("Request to update plate for bike {BikeId} to {Plate}.", id, dto.Plate);
                var bike = await _bikeService.UpdatePlateAsync(id, dto) ?? throw new DomainException(DomainErrors.Bike.NotFound);
                _logger.LogInformation("Bike {BikeId} plate updated successfully to {Plate}.", id, dto.Plate);
                return Ok();
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Failed to update plate for bike {BikeId} due to domain validation error {Message}.", id, ex.Error.Message);
                return BadRequest(new { message = ex.Error.Message });
            }
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DomainError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DomainError), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BikeDto>> GetById(Guid id)
        {
            try
            {
                _logger.LogInformation("Fetching bike {BikeId}.", id);
                var bike = await _bikeService.GetByIdAsync(id) ?? throw new DomainException(DomainErrors.Bike.NotFound);
                _logger.LogInformation("Bike {BikeId} retrieved successfully.", id);
                return Ok(bike);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Failed to delete bike {BikeId} due to domain validation error {Message}.", id, ex.Error.Message);
                return BadRequest(new { message = ex.Error.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DomainError), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                _logger.LogInformation("Request to delete bike {BikeId}.", id);
                await _bikeService.DeleteAsync(id);
                _logger.LogInformation("Bike {BikeId} deleted successfully.", id);
                return Ok();
            }
            catch (DomainException ex) when (ex.Error == DomainErrors.Bike.NotFound)
            {
                _logger.LogWarning(ex, "Failed to delete bike {BikeId} because it was not found.", id);
                return BadRequest(new { message = ex.Error.Message });
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Failed to delete bike {BikeId} due to domain validation error {Message}.", id, ex.Error.Message);
                return BadRequest(new { message = ex.Error.Message });
            }
        }
    }
}
