using FleetRent.Domain.Entities;
using FleetRent.Domain.Interfaces;
using FleetRent.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FleetRent.Infrastructure.Repositories
{
    public class NotificationRepository(FleetRentDbContext context) : INotificationRepository
    {
        private readonly FleetRentDbContext _context = context;

        public async Task AddAsync(Notification notification, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(notification);
            await _context.Notifications.AddAsync(notification, ct);
        }

        public async Task<List<Notification>> GetLatestAsync(int take, CancellationToken ct = default)
        {
            if (take <= 0)            
                return [];            

            return await _context.Notifications
                .AsNoTracking()
                .OrderByDescending(n => n.CreatedAt)
                .Take(take)
                .ToListAsync(ct);
        }
    }
}
