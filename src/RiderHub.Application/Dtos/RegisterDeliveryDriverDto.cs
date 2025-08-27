using Microsoft.AspNetCore.Http;
using RiderHub.Domain.Enums;

namespace RiderHub.Application.Dtos
{
    public class RegisterDeliveryDriverDto
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string CNPJ { get; set; }
        public required DateTime DateOfBirth { get; set; }
        public required string DriverLicenseNumber { get; set; }
        public required DriverLicenseTypeEnum DriverLicenseType { get; set; }
        public required IFormFile DriverLicenseImage { get; set; } 
    }
}
