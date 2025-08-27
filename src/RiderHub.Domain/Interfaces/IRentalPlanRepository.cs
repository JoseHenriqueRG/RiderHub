using RiderHub.Domain.Entities;

namespace RiderHub.Domain.Interfaces
{
    public interface IRentalPlanRepository
    {
        Task<RentalPlan?> GetByDaysAsync(int days);
    }
}
