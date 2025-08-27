namespace RiderHub.Application.Dtos
{
    public class CreateMotorcycleDto
    {
        public int Year { get; set; }
        public required string Model { get; set; }
        public required string LicensePlate { get; set; }
    }
}
