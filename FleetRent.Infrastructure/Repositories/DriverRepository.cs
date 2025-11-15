using FleetRent.Domain.Entities;
using FleetRent.Domain.Interfaces;
using FleetRent.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FleetRent.Infrastructure.Repositories
{
    public class DriverRepository : IDriverRepository
    {
        private readonly FleetRentDbContext _context;

        public DriverRepository(FleetRentDbContext context) => _context = context;

        public async Task AddAsync(Driver driver, CancellationToken ct = default)
        {
            await _context.Drivers.AddAsync(driver, ct);
        }

        public async Task<IEnumerable<Driver>> GetAllAsync()
        {
            var query = _context.Drivers.AsQueryable();

            return await query.ToListAsync();
        }

        public async Task<Driver> GetByLicenseNumberAsync(string licenseNumber, CancellationToken ct = default)
        {
            return await _context.Drivers
                .FirstOrDefaultAsync(d => d.LicenseNumber == licenseNumber, ct);
        }

        public async Task<Driver> GetByCnpjAsync(string cnpj, CancellationToken ct = default)
        {
            return await _context.Drivers
                .FirstOrDefaultAsync(d => d.Cnpj == cnpj, ct);
        }

        public async Task<Driver> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Drivers.FindAsync(new object[] { id }, ct);
        }

        public void Update(Driver driver)
        {
            _context.Drivers.Update(driver);
        }
    }
}