using FleetRent.Domain.Entities;

namespace FleetRent.Domain.Interfaces
{
    public interface IDriverRepository
    {
        Task AddAsync(Driver driver, CancellationToken ct = default);
        Task<IEnumerable<Driver>> GetAllAsync();
        Task<Driver> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Driver> GetByCnpjAsync(string cnpj, CancellationToken ct = default);
        Task<Driver> GetByLicenseNumberAsync(string licenseNumber, CancellationToken ct = default);
        void Update(Driver driver);
    }
}
