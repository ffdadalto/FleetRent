using FleetRent.Domain.Entities;

namespace FleetRent.Domain.Interfaces
{
    public interface IBikeRepository
    {
        Task AddAsync(Bike bike, CancellationToken ct = default);
        Task<Bike> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Bike> GetByPlateAsync(string plate, CancellationToken ct = default);
        Task<List<Bike>> GetAsync(string? plate, CancellationToken ct = default);
        void Update(Bike bike);
        void Remove(Bike bike);
    }   
}
