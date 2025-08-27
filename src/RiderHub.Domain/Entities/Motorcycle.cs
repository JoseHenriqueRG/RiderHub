namespace RiderHub.Domain.Entities
{
    public class Motorcycle
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public required string Model { get; set; }
        public required string LicensePlate { get; set; }
    }
}
