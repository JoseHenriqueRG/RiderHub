using RiderHub.Application.Dtos;
using RiderHub.Domain.Entities;

namespace RiderHub.Application.Interfaces
{
    public interface IRentalService
    {
        Task<Rental> CreateRentalAsync(CreateRentalDto dto);
        Task<decimal> CalculateRentalCostAsync(ReturnRentalDto dto);
    }
}
