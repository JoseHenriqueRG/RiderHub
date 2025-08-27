using Microsoft.EntityFrameworkCore;
using RiderHub.Domain.Entities;
using RiderHub.Domain.Interfaces;
using RiderHub.Infrastructure.Context;

namespace RiderHub.Infrastructure.Repositories
{
    public class DeliveryDriverRepository : IDeliveryDriverRepository
    {
        private readonly RiderHubContext _context;

        public DeliveryDriverRepository(RiderHubContext context)
        {
            _context = context;
        }

        public async Task AddAsync(DeliveryDriver deliveryDriver)
        {
            await _context.DeliveryDrivers.AddAsync(deliveryDriver);
            await _context.SaveChangesAsync();
        }

        public async Task<DeliveryDriver?> GetByCNPJAsync(string cnpj)
        {
            return await _context.DeliveryDrivers.FirstOrDefaultAsync(x => x.CNPJ == cnpj);
        }

        public async Task<DeliveryDriver?> GetByDriverLicenseNumberAsync(string driverLicenseNumber)
        {
            return await _context.DeliveryDrivers.FirstOrDefaultAsync(x => x.DriverLicenseNumber == driverLicenseNumber);
        }

        public async Task<DeliveryDriver?> GetByIdAsync(int deliveryDriverId)
        {
            return await _context.DeliveryDrivers.FirstOrDefaultAsync(x => x.Id == deliveryDriverId);
        }

        public async Task UpdateAsync(DeliveryDriver deliveryDriver)
        {
            _context.DeliveryDrivers.Update(deliveryDriver);
            await _context.SaveChangesAsync();
        }
    }
}
