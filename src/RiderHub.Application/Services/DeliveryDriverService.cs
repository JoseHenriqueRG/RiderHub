using RiderHub.Application.Dtos;
using RiderHub.Application.Interfaces;
using RiderHub.Domain.Entities;
using RiderHub.Domain.Interfaces;
using RiderHub.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using RiderHub.Domain.Exceptions;

namespace RiderHub.Application.Services
{
    public class DeliveryDriverService : IDeliveryDriverService
    {
        private readonly IDeliveryDriverRepository _deliveryDriverRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DeliveryDriverService(IDeliveryDriverRepository deliveryDriverRepository, IWebHostEnvironment webHostEnvironment)
        {
            _deliveryDriverRepository = deliveryDriverRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<DeliveryDriver> RegisterDeliveryDriverAsync(RegisterDeliveryDriverDto dto)
        {
            if (dto.DriverLicenseImage == null || dto.DriverLicenseImage.Length == 0)
            {
                throw new BusinessRuleException("Driver license image is required.");
            }

            // Validate CNPJ
            if (!IsValidCnpj(dto.CNPJ))
            {
                throw new BusinessRuleException("Invalid CNPJ format.");
            }

            var existingDriverByCnpj = await _deliveryDriverRepository.GetByCNPJAsync(dto.CNPJ);
            if (existingDriverByCnpj != null)
            {
                throw new DuplicateCnpjException();
            }

            // Validate CNH
            if (!Enum.IsDefined(typeof(DriverLicenseTypeEnum), dto.DriverLicenseType))
            {
                throw new BusinessRuleException("Tipo de CNH inválido.");
            }

            var existingDriverByCnh = await _deliveryDriverRepository.GetByDriverLicenseNumberAsync(dto.DriverLicenseNumber);
            if (existingDriverByCnh != null)
            {
                throw new DuplicateCnhException();
            }

            var cnhImageUrl = await _SaveCnhImageAsync(dto.DriverLicenseImage);

            var deliveryDriver = new DeliveryDriver
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = UserRoleEnum.DeliveryDriver,
                CNPJ = dto.CNPJ,
                DateOfBirth = dto.DateOfBirth,
                DriverLicenseNumber = dto.DriverLicenseNumber,
                DriverLicenseType = dto.DriverLicenseType,
                DriverLicenseImage = cnhImageUrl
            };

            await _deliveryDriverRepository.AddAsync(deliveryDriver);

            return deliveryDriver;
        }

        public async Task UpdateCnhImageAsync(UpdateCnhImageDto dto)
        {
            var deliveryDriver = await _deliveryDriverRepository.GetByIdAsync(dto.DeliveryDriverId);
            if (deliveryDriver == null)
            {
                throw new EntityNotFoundException(nameof(DeliveryDriver), dto.DeliveryDriverId);
            }

            var cnhImageUrl = await _SaveCnhImageAsync(dto.CnhImageFile);

            // Update DriverLicenseImage path in the database
            deliveryDriver.DriverLicenseImage = cnhImageUrl;
            await _deliveryDriverRepository.UpdateAsync(deliveryDriver);
        }

        public async Task<DeliveryDriver?> GetDeliveryDriverById(int id)
        {
            return await _deliveryDriverRepository.GetByIdAsync(id);
        }

        private async Task<string> _SaveCnhImageAsync(IFormFile imageFile)
        {
            // Validate file format
            var allowedExtensions = new[] { ".png", ".bmp" };
            var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new BusinessRuleException("Invalid file format. Only PNG and BMP are allowed.");
            }

            // Save image to local storage
            var fileName = Guid.NewGuid().ToString() + fileExtension;
            var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "cnh_images");
            var filePath = Path.Combine(uploadPath, fileName);

            // Ensure the directory exists
            Directory.CreateDirectory(uploadPath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/cnh_images/{fileName}";
        }

        private bool IsValidCnpj(string cnpj)
        {
            return cnpj.Length == 14 && long.TryParse(cnpj, out _);
        }

    }
}