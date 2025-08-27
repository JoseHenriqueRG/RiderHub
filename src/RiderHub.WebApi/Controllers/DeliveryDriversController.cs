using Microsoft.AspNetCore.Mvc;
using RiderHub.Application.Dtos;
using RiderHub.Application.Interfaces;
namespace RiderHub.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryDriversController : ControllerBase
    {
        private readonly IDeliveryDriverService _deliveryDriverService;

        public DeliveryDriversController(IDeliveryDriverService deliveryDriverService)
        {
            _deliveryDriverService = deliveryDriverService;
        }

        [HttpPost("register")]
        //[Authorize(Roles = "Admin,DeliveryDriver")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterDeliveryDriver([FromForm] RegisterDeliveryDriverDto dto)
        {
            var deliveryDriver = await _deliveryDriverService.RegisterDeliveryDriverAsync(dto);
            
            return CreatedAtAction(
                nameof(GetDeliveryDriverById),
                new { id = deliveryDriver.Id },
                deliveryDriver
            );
        }

        [HttpPut("cnh-image")]
        //[Authorize(Roles = "Admin,DeliveryDriver")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCnhImage([FromForm] UpdateCnhImageDto dto)
        {
            await _deliveryDriverService.UpdateCnhImageAsync(dto);
            return Ok();
        }

        // GET para consultar o driver por ID
        [HttpGet("{id:int}")]
        //[Authorize(Roles = "Admin,DeliveryDriver")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDeliveryDriverById(int id)
        {
            var deliveryDriver = await _deliveryDriverService.GetDeliveryDriverById(id);
            return deliveryDriver is null ? NotFound() : Ok(deliveryDriver);
        }
    }
}
