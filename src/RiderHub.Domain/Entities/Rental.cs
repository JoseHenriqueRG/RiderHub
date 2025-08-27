namespace RiderHub.Domain.Entities
{
    public class Rental
    {
        public int Id { get; set; }

        // Datas
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ExpectedReturnDate { get; set; }

        // Plano e valor da diária
        public int PlanDays { get; set; }
        public decimal DailyRate { get; set; }

        // Relação com o entregador
        public int DeliveryDriverId { get; set; }
        public required DeliveryDriver DeliveryDriver { get; set; }

        // Relação com a moto
        public int MotorcycleId { get; set; }
        public required Motorcycle Motorcycle { get; set; }
    }
}
