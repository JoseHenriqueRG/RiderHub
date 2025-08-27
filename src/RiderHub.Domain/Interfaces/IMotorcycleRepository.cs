using RiderHub.Domain.Entities;

namespace RiderHub.Domain.Interfaces
{
    public interface IMotorcycleRepository
    {
        Task AddAsync(Motorcycle motorcycle);
        Task<Motorcycle?> GetByIdAsync(int id);
        Task<List<Motorcycle>> GetAllAsync(string? LicensePlate);
        Task UpdateAsync(Motorcycle motorcycle);
        Task<bool> HasRentalsAsync(int motorcycleId);
        Task DeleteAsync(Motorcycle motorcycle);
        Task<bool> ExistsByLicensePlateAsync(string licensePlate);
    }
}
