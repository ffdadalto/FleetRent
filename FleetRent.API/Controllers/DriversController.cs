using FleetRent.Application.Drivers;
using FleetRent.Application.Drivers.Dtos;
using FleetRent.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FleetRent.Api.Controllers
{
    [ApiController]
    [Route("drivers")]
    public class DriversController(IDriverService driverService, ILogger<DriversController> logger) : ControllerBase
    {
        private readonly IDriverService _driverService = driverService;
        private readonly ILogger<DriversController> _logger = logger;

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(DomainError), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateDriverDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for creating driver: {ModelState}", ModelState);
                    throw new DomainException(DomainErrors.Driver.InvalidData);
                }

                _logger.LogInformation("Received request to create driver with identifier {Identifier}.", dto.Identifier);
                var driver = await _driverService.CreateAsync(dto);
                _logger.LogInformation("Driver {DriverId} created successfully with identifier {Identifier}.", driver.Id, driver.Identifier);
                return StatusCode(StatusCodes.Status201Created);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(ex, "Failed to create driver due to domain validation error {Message}.", ex.Error.Message);
                return BadRequest(new { message = ex.Error.Message });
            }
        }

        [HttpPost("{id:guid}/license")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(DomainError), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadLicense(Guid id, [FromBody] UpdateDriverLicenseImageDto img, CancellationToken ct)
        {   
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for updating driver license: {ModelState}", ModelState);
                    throw new DomainException(DomainErrors.Driver.InvalidData);
                }

                _logger.LogInformation("Received request to upload license image for driver {DriverId}.", id);

                if (!TryExtractImagePayload(img.ImageBase64, out var bytes, out var contentType, out var validationMessage))
                {
                    _logger.LogWarning("License upload rejected for driver {DriverId} due to invalid payload: {Reason}.", id, validationMessage);
                    throw new DomainException(DomainErrors.Driver.InvalidData);
                }

                await using var stream = new MemoryStream(bytes);

                // Generate a deterministic filename so newer uploads overwrite the same resource if desired.
                var extension = contentType == "image/png" ? ".png" : ".bmp";
                var fileName = $"{id}_license{extension}";

                _logger.LogInformation("Uploading license image for driver {DriverId} with file {FileName}.", id, fileName);
                var updated = await _driverService.UpdateLicenseImageAsync(id, stream, fileName, contentType, ct);
                if (updated is null)
                {
                    _logger.LogWarning("Driver {DriverId} not found when uploading license image.", id);
                    return BadRequest();
                }

                _logger.LogInformation("License image for driver {DriverId} updated successfully.", id);
                return StatusCode(StatusCodes.Status201Created);
            }
            catch (DomainException ex)
            {
                if (IsStorageError(ex))
                {
                    _logger.LogError(ex, "Storage error while uploading license image for driver {DriverId} with error {Message}.", id, ex.Error.Message);
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Error.Message });
                }

                _logger.LogWarning(ex, "Domain validation error while uploading license image for driver {DriverId}: {Message}.", id, ex.Error.Message);
                return BadRequest(new { message = ex.Error.Message });
            }
        }

        private static bool TryExtractImagePayload(string base64Input, out byte[] bytes, out string contentType, out string validationMessage)
        {
            const string png = "image/png";
            const string bmp = "image/bmp";

            contentType = png;
            bytes = Array.Empty<byte>();

            var payload = base64Input.Trim();
            var commaIndex = payload.IndexOf(',');

            // Support data URLs, e.g., data:image/png;base64,AAAA
            if (payload.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && commaIndex > 0)
            {
                var meta = payload[5..commaIndex];
                var parts = meta.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length > 0)
                {
                    contentType = parts[0].ToLowerInvariant();
                }

                payload = payload[(commaIndex + 1)..];
            }
            else
            {
                payload = payload.Trim();
            }

            if (!string.Equals(contentType, png, StringComparison.OrdinalIgnoreCase) && !string.Equals(contentType, bmp, StringComparison.OrdinalIgnoreCase))
            {
                validationMessage = "Only PNG or BMP are allowed.";
                return false;
            }

            try
            {
                bytes = Convert.FromBase64String(payload);
            }
            catch (FormatException)
            {
                validationMessage = "Invalid base64 string.";
                return false;
            }

            if (bytes.Length == 0)
            {
                validationMessage = "Image is empty.";
                return false;
            }

            validationMessage = null;
            contentType = contentType.ToLowerInvariant();
            return true;
        }

        private static bool IsStorageError(DomainException ex) =>
            ex.Error == DomainErrors.Storage.UploadDirectoryUnavailable ||
            ex.Error == DomainErrors.Storage.UploadPermissionDenied ||
            ex.Error == DomainErrors.Storage.UploadDirectoryNotFound ||
            ex.Error == DomainErrors.Storage.UploadIoFailure ||
            ex.Error == DomainErrors.Storage.UploadUnexpected;
    }
}
