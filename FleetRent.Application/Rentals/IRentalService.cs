using FleetRent.Application.Rentals.Dtos;

namespace FleetRent.Application.Rentals
{
    public interface IRentalService
    {
        Task<RentalDto> CreateAsync(CreateRentalDto dto, CancellationToken ct = default);
        Task<RentalDto> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<RentalDto> CloseAsync(Guid id, CloseRentalDto dto, CancellationToken ct = default);
    }
}
