using Microsoft.EntityFrameworkCore;
using RiderHub.Domain.Entities;
using RiderHub.Domain.Interfaces;
using RiderHub.Infrastructure.Context;

namespace RiderHub.Infrastructure.Repositories
{
    public class RentalPlanRepository : IRentalPlanRepository
    {
        private readonly RiderHubContext _context;

        public RentalPlanRepository(RiderHubContext context)
        {
            _context = context;
        }

        public async Task<RentalPlan?> GetByDaysAsync(int days)
        {
            return await _context.RentalPlans.FirstOrDefaultAsync(rp => rp.Days == days);
        }
    }
}
