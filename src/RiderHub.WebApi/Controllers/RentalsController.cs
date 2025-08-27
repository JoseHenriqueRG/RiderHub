using Microsoft.AspNetCore.Mvc;
using RiderHub.Application.Dtos;
using RiderHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace RiderHub.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RentalsController : ControllerBase
    {
        private readonly IRentalService _rentalService;

        public RentalsController(IRentalService rentalService)
        {
            _rentalService = rentalService;
        }

        [HttpPost]
        //[Authorize(Roles = "Admin,DeliveryDriver")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRental([FromBody] CreateRentalDto dto)
        {
            var rental = await _rentalService.CreateRentalAsync(dto);
            return CreatedAtAction(nameof(CreateRental), new { id = rental.Id }, rental);
        }

        [HttpPut("calculate-cost")]
        //[Authorize(Roles = "Admin,DeliveryDriver")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CalculateRentalCost([FromBody] ReturnRentalDto dto)
        {
            var totalCost = await _rentalService.CalculateRentalCostAsync(dto);
            return Ok(new { TotalCost = totalCost });
        }
    }
}
