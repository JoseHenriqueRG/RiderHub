namespace RiderHub.Application.Dtos
{
    public class CreateRentalDto
    {
        public int DeliveryDriverId { get; set; }
        public int MotorcycleId { get; set; }
        public int PlanDays { get; set; }
    }
}
