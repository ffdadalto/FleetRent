using FleetRent.Domain.Entities;
using FleetRent.Domain.Interfaces;
using FleetRent.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FleetRent.Infrastructure.Repositories
{
    public class RentalRepository(FleetRentDbContext context) : IRentalRepository
    {
        private readonly FleetRentDbContext _context = context;

        public async Task AddAsync(Rental rental, CancellationToken ct = default)
        {
            await _context.Rentals.AddAsync(rental, ct);
        }

        public async Task<Rental> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Rentals
                .FirstOrDefaultAsync(r => r.Id == id, ct);
        }

        public async Task<bool> HasActiveRentalForBikeAsync(Guid bikeId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
        {
            return await _context.Rentals
                .Where(r => r.BikeId == bikeId)
                .AnyAsync(r =>                    
                    r.StartDate <= endDate &&
                    (r.EndDate ?? r.PlannedEndDate) >= startDate,
                    ct);
        }

        public async Task<int> CountAsync(CancellationToken ct = default)
        {
            return await _context.Rentals.CountAsync(ct);
        }
    }
}
