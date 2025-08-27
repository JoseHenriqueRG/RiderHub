using RiderHub.Domain.Entities;
using RiderHub.Domain.Interfaces;
using RiderHub.Infrastructure.Context;

namespace RiderHub.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly RiderHubContext _context;

        public NotificationRepository(RiderHubContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }
    }
}
