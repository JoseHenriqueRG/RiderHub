using RiderHub.Domain.Enums;

namespace RiderHub.Domain.Entities
{
    public class DeliveryDriver : User
    {
        public required string CNPJ { get; set; }
        public DateTime DateOfBirth { get; set; }
        public required string DriverLicenseNumber { get; set; }
        public required DriverLicenseTypeEnum DriverLicenseType { get; set; }
        public required string DriverLicenseImage { get; set; }
    }
}
