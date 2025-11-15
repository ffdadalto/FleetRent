using FleetRent.Domain.Entities;

namespace FleetRent.Domain.Interfaces
{
    public interface IRentalRepository
    {
        Task AddAsync(Rental rental, CancellationToken ct = default);
        Task<Rental> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> HasActiveRentalForBikeAsync(Guid bikeId, DateTime startDate, DateTime endDate, CancellationToken ct = default);
        Task<int> CountAsync(CancellationToken ct = default);
    }
}
