using FleetRent.Domain.Entities;

namespace FleetRent.Domain.Interfaces
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification, CancellationToken ct = default);
        Task<List<Notification>> GetLatestAsync(int take, CancellationToken ct = default);
    }
}
