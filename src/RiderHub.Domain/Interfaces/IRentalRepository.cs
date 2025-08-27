using RiderHub.Domain.Entities;

namespace RiderHub.Domain.Interfaces
{
    public interface IRentalRepository
    {
        Task AddAsync(Rental rental);
        Task<bool> IsMotorcycleAvailableAsync(int motorcycleId, DateTime startDate, DateTime endDate);
        Task<Rental?> GetByIdAsync(int id);
        Task UpdateAsync(Rental rental);
    }
}
