using RiderHub.Application.Dtos;
using RiderHub.Application.Interfaces;
using RiderHub.Domain.Entities;
using RiderHub.Domain.Exceptions;
using RiderHub.Domain.Interfaces;
using System.Text.Json;

namespace RiderHub.Application.Services
{
    public class MotorcycleService : IMotorcycleService
    {
        private readonly IMotorcycleRepository _motorcycleRepository;
        private readonly IMessagePublisher _messagePublisher;

        public MotorcycleService(IMotorcycleRepository motorcycleRepository, IMessagePublisher messagePublisher)
        { 
            _motorcycleRepository = motorcycleRepository;
            _messagePublisher = messagePublisher;
        }

        public async Task<Motorcycle> CreateMotorcycleAsync(CreateMotorcycleDto dto)
        {
            var exists = await _motorcycleRepository.ExistsByLicensePlateAsync(dto.LicensePlate);
            if (exists)
            {
                throw new BusinessRuleException("A motorcycle with this license plate already exists.");
            }

            var motorcycle = new Motorcycle
            {
                Model = dto.Model,
                Year = dto.Year,
                LicensePlate = dto.LicensePlate
            };

            await _motorcycleRepository.AddAsync(motorcycle);

            var message = JsonSerializer.Serialize(motorcycle);
            _messagePublisher.Publish(message);

            return motorcycle;
        }

        public async Task<IEnumerable<Motorcycle>> GetMotorcyclesAsync(string? plate = null)
        {
            return await _motorcycleRepository.GetAllAsync(plate);
        }

        public async Task UpdateMotorcycleLicensePlateAsync(UpdateMotorcycleLicensePlateDto dto)
        {
            var exists = await _motorcycleRepository.ExistsByLicensePlateAsync(dto.LicensePlate);
            if (exists)
            {
                throw new BusinessRuleException("A motorcycle with this license plate already exists.");
            }

            var motorcycle = await _motorcycleRepository.GetByIdAsync(dto.Id);
            if (motorcycle == null)
            {
                throw new EntityNotFoundException(nameof(Motorcycle), dto.Id);
            }

            motorcycle.LicensePlate = dto.LicensePlate;

            await _motorcycleRepository.UpdateAsync(motorcycle);
        }

        public async Task DeleteMotorcycleAsync(int id)
        {
            var motorcycle = await _motorcycleRepository.GetByIdAsync(id);
            if (motorcycle == null)
            {
                throw new EntityNotFoundException(nameof(Motorcycle), id);
            }

            var hasRentals = await _motorcycleRepository.HasRentalsAsync(id);
            if (hasRentals)
            {
                throw new BusinessRuleException("Motorcycle cannot be deleted because it has associated rentals.");
            }

            await _motorcycleRepository.DeleteAsync(motorcycle);
        }

        public async Task<Motorcycle?> GetMotorcycleByIdAsync(int id)
        {
            return await _motorcycleRepository.GetByIdAsync(id);
        }
    }
}
