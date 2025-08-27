using RiderHub.Application.Dtos;
using RiderHub.Domain.Entities;

namespace RiderHub.Application.Interfaces
{
    public interface IDeliveryDriverService
    {
        Task<DeliveryDriver?> GetDeliveryDriverById(int id);
        Task<DeliveryDriver> RegisterDeliveryDriverAsync(RegisterDeliveryDriverDto dto);
        Task UpdateCnhImageAsync(UpdateCnhImageDto dto);
    }
}
