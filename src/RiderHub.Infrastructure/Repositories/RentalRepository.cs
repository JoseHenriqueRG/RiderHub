using Microsoft.EntityFrameworkCore;
using RiderHub.Domain.Entities;
using RiderHub.Domain.Interfaces;
using RiderHub.Infrastructure.Context;

namespace RiderHub.Infrastructure.Repositories
{
    public class RentalRepository : IRentalRepository
    {
        private readonly RiderHubContext _context;

        public RentalRepository(RiderHubContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Rental rental)
        {
            await _context.Rentals.AddAsync(rental);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsMotorcycleAvailableAsync(int motorcycleId, DateTime startDate, DateTime endDate)
        {
            return !await _context.Rentals.AnyAsync(r =>
                r.MotorcycleId == motorcycleId &&
                ((startDate < r.EndDate && endDate > r.StartDate) ||
                 (startDate == r.StartDate && endDate == r.EndDate)));
        }

        public async Task<Rental?> GetByIdAsync(int id)
        {
            return await _context.Rentals.FindAsync(id);
        }

        public async Task UpdateAsync(Rental rental)
        {
            _context.Rentals.Update(rental);
            await _context.SaveChangesAsync();
        }
    }
}