using RiderHub.Application.Dtos;
using RiderHub.Application.Interfaces;
using RiderHub.Domain.Entities;
using RiderHub.Domain.Enums;
using RiderHub.Domain.Exceptions;
using RiderHub.Domain.Interfaces;

namespace RiderHub.Application.Services
{
    public class RentalService : IRentalService
    {
        private readonly IRentalRepository _rentalRepository;
        private readonly IMotorcycleRepository _motorcycleRepository;
        private readonly IDeliveryDriverRepository _deliveryDriverRepository;
        private readonly IRentalPlanRepository _rentalPlanRepository;

        public RentalService(IRentalRepository rentalRepository, IMotorcycleRepository motorcycleRepository, IDeliveryDriverRepository deliveryDriverRepository, IRentalPlanRepository rentalPlanRepository)
        {
            _rentalRepository = rentalRepository;
            _motorcycleRepository = motorcycleRepository;
            _deliveryDriverRepository = deliveryDriverRepository;
            _rentalPlanRepository = rentalPlanRepository;
        }

        public async Task<Rental> CreateRentalAsync(CreateRentalDto dto)
        {
            // Validate DeliveryDriverId and MotorcycleId
            var deliveryDriver = await _deliveryDriverRepository.GetByIdAsync(dto.DeliveryDriverId);
            if (deliveryDriver == null)
            {
                throw new EntityNotFoundException(nameof(Rental), dto.DeliveryDriverId);
            }

            // Rule 3: Only delivery drivers with CNH category A can rent
            if (deliveryDriver.DriverLicenseType != DriverLicenseTypeEnum.A 
                && deliveryDriver.DriverLicenseType != DriverLicenseTypeEnum.AB)
            {
                throw new BusinessRuleException("Only delivery drivers with CNH category A can rent a motorcycle.");
            }

            var motorcycle = await _motorcycleRepository.GetByIdAsync(dto.MotorcycleId);
            if (motorcycle == null)
            {
                throw new EntityNotFoundException(nameof(Motorcycle), dto.MotorcycleId);
            }

            // Get daily rate from plan
            var rentalPlan = await _rentalPlanRepository.GetByDaysAsync(dto.PlanDays);
            if (rentalPlan == null)
            {
                throw new BusinessRuleException("Invalid rental plan days.");
            }

            // Rule 2: Rental start date is the first day after creation date
            var startDate = DateTime.UtcNow.Date.AddDays(1);
            var expectedReturnDate = startDate.AddDays(dto.PlanDays);

            // Check motorcycle availability
            var isAvailable = await _rentalRepository.IsMotorcycleAvailableAsync(dto.MotorcycleId, startDate, expectedReturnDate);
            if (!isAvailable)
            {
                throw new BusinessRuleException("Motorcycle is not available for the requested period.");
            }

            var rental = new Rental
            {
                DeliveryDriverId = dto.DeliveryDriverId,
                DeliveryDriver = deliveryDriver,
                MotorcycleId = dto.MotorcycleId,
                Motorcycle = motorcycle,
                StartDate = startDate,
                ExpectedReturnDate = expectedReturnDate,
                PlanDays = dto.PlanDays,
                DailyRate = rentalPlan.DailyRate,
                EndDate = expectedReturnDate
            };

            await _rentalRepository.AddAsync(rental);

            return rental;
        }

        public async Task<decimal> CalculateRentalCostAsync(ReturnRentalDto dto)
        {
            var rental = await _rentalRepository.GetByIdAsync(dto.RentalId);
            if (rental == null)
            {
                throw new EntityNotFoundException(nameof(Rental), dto.RentalId);
            }

            if (dto.ReturnDate < rental.StartDate)
            {
                throw new BusinessRuleException("Return date cannot be before the rental start date.");
            }

            // Update the actual EndDate of the rental
            rental.EndDate = dto.ReturnDate;
            await _rentalRepository.UpdateAsync(rental);

            decimal totalCost = 0;
            var actualDaysRented = (decimal)(dto.ReturnDate - rental.StartDate).TotalDays;

            // Early Return
            if (dto.ReturnDate < rental.ExpectedReturnDate)
            {
                var remainingDays = (decimal)(rental.ExpectedReturnDate - dto.ReturnDate).TotalDays;
                decimal penaltyPercentage = 0;

                if (rental.PlanDays == 7)
                {
                    penaltyPercentage = 0.20m; // 20%
                }
                else if (rental.PlanDays == 15)
                {
                    penaltyPercentage = 0.40m; // 40%
                }

                totalCost = (actualDaysRented * rental.DailyRate) + (remainingDays * rental.DailyRate * penaltyPercentage);
            }
            // Late Return
            else if (dto.ReturnDate > rental.ExpectedReturnDate)
            {
                var extraDays = (decimal)(dto.ReturnDate - rental.ExpectedReturnDate).TotalDays;
                totalCost = (rental.PlanDays * rental.DailyRate) + (extraDays * 50.00m); // R$50.00 per additional day
            }
            // On-Time Return
            else
            {
                totalCost = rental.PlanDays * rental.DailyRate;
            }

            return totalCost;
        }
    }
}