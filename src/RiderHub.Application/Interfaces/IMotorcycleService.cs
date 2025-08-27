using RiderHub.Application.Dtos;
using RiderHub.Domain.Entities;

namespace RiderHub.Application.Interfaces
{
    public interface IMotorcycleService
    {
        Task<Motorcycle> CreateMotorcycleAsync(CreateMotorcycleDto dto);
        Task<IEnumerable<Motorcycle>> GetMotorcyclesAsync(string? plate = null);
        Task<Motorcycle?> GetMotorcycleByIdAsync(int id);
        Task UpdateMotorcycleLicensePlateAsync(UpdateMotorcycleLicensePlateDto dto);
        Task DeleteMotorcycleAsync(int id);
    }
}
