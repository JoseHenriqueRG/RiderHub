using Microsoft.EntityFrameworkCore;
using RiderHub.Domain.Entities;
using RiderHub.Domain.Interfaces;
using RiderHub.Infrastructure.Context;

namespace RiderHub.Infrastructure.Repositories
{
    public class MotorcycleRepository : IMotorcycleRepository
    {
        private readonly RiderHubContext _context;

        public MotorcycleRepository(RiderHubContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Motorcycle motorcycle)
        {
            await _context.Motorcycles.AddAsync(motorcycle);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Motorcycle>> GetAllAsync(string? LicensePlate)
        {
            var query = _context.Motorcycles.AsQueryable();

            if (!string.IsNullOrEmpty(LicensePlate))
            {
                query = query.Where(m => m.LicensePlate.Contains(LicensePlate));
            }

            return await query.ToListAsync();
        }

        public async Task<Motorcycle?> GetByIdAsync(int id)
        {
            return await _context.Motorcycles.FindAsync(id);
        }

        public async Task UpdateAsync(Motorcycle motorcycle)
        {
            _context.Motorcycles.Update(motorcycle);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasRentalsAsync(int motorcycleId)
        {
            return await _context.Rentals.AnyAsync(r => r.MotorcycleId == motorcycleId);
        }

        public async Task DeleteAsync(Motorcycle motorcycle)
        {
            _context.Motorcycles.Remove(motorcycle);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByLicensePlateAsync(string licensePlate)
        {
            return await _context.Motorcycles.AnyAsync(m => m.LicensePlate == licensePlate);
        }
    }
}
