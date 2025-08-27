using RiderHub.Domain.Entities;

namespace RiderHub.Domain.Interfaces
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
    }
}
