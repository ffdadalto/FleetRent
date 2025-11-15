using FleetRent.Domain.Entities;
using FleetRent.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using FleetRent.Infrastructure.Data;

namespace FleetRent.Infrastructure.Repositories
{
    public class BikeRepository(FleetRentDbContext context) : IBikeRepository
    {
        private readonly FleetRentDbContext _context = context;

        public async Task AddAsync(Bike bike, CancellationToken ct = default)
        {
            await _context.Bikes.AddAsync(bike, ct);
        }

        public async Task<Bike> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Bikes
                .Include(b => b.Rentals)
                .FirstOrDefaultAsync(b => b.Id == id, ct);
        }

        public async Task<Bike> GetByPlateAsync(string plate, CancellationToken ct = default)
        {
            plate = plate.Trim().ToUpperInvariant();
            return await _context.Bikes
                .FirstOrDefaultAsync(b => b.Plate == plate, ct);
        }

        public async Task<List<Bike>> GetAsync(string? plate, CancellationToken ct = default)
        {
            var query = _context.Bikes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(plate))
            {
                var normalized = plate.Trim().ToUpperInvariant();
                query = query.Where(b => b.Plate == normalized);
            }

            return await query.ToListAsync(ct);
        }

        public void Update(Bike bike)
        {
            _context.Bikes.Update(bike);
        }

        public void Remove(Bike bike)
        {
            _context.Bikes.Remove(bike);
        }
    }
}
