using Microsoft.AspNetCore.Http;

namespace RiderHub.Application.Dtos
{
    public class UpdateCnhImageDto
    {
        public int DeliveryDriverId { get; set; }
        public required IFormFile CnhImageFile { get; set; }
    }
}
