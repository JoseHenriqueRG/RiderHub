
using RiderHub.Domain.Entities;

namespace RiderHub.Domain.Interfaces
{
    public interface IDeliveryDriverRepository
    {
        Task AddAsync(DeliveryDriver deliveryDriver);
        Task<DeliveryDriver?> GetByCNPJAsync(string cnpj);
        Task<DeliveryDriver?> GetByDriverLicenseNumberAsync(string driverLicenseNumber);
        Task<DeliveryDriver?> GetByIdAsync(int deliveryDriverId);
        Task UpdateAsync(DeliveryDriver deliveryDriver);
    }
}
