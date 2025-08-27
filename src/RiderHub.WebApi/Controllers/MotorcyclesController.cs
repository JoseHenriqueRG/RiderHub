using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RiderHub.Application.Dtos;
using RiderHub.Application.Interfaces;
using RiderHub.Domain.Exceptions;

namespace RiderHub.WebApi.Controllers
{
    [Route("api/motos")]
    [ApiController]
    public class MotorcyclesController : ControllerBase
    {
        private readonly IMotorcycleService _motorcycleService;

        public MotorcyclesController(IMotorcycleService motorcycleService)
        {
            _motorcycleService = motorcycleService;
        }

        [HttpPost]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateMotorcycle([FromBody] CreateMotorcycleDto dto)
        {
            var motorcycle = await _motorcycleService.CreateMotorcycleAsync(dto);
            return CreatedAtAction(nameof(GetMotorcycleById), new { id = motorcycle.Id }, motorcycle);
        }

        [HttpGet]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMotorcycles([FromQuery] string? plate)
        {
            var motorcycles = await _motorcycleService.GetMotorcyclesAsync(plate);
            return Ok(motorcycles);
        }

        [HttpPut("license-plate")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateMotorcycleLicensePlate([FromBody] UpdateMotorcycleLicensePlateDto dto)
        {
            await _motorcycleService.UpdateMotorcycleLicensePlateAsync(dto); 
            return Ok(new { message = "License plate updated successfully" });

        }

        [HttpGet("{id:int}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMotorcycleById(int id)
        {
            if (id < 1)
                return BadRequest(new { message = "Invalid ID" });

            var motorcycle = await _motorcycleService.GetMotorcycleByIdAsync(id);
            return motorcycle is null
                ? NotFound(new { message = "Motorcycle not found" })
                : Ok(motorcycle);
        }

        [HttpDelete("{id:int}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteMotorcycle(int id)
        {
            await _motorcycleService.DeleteMotorcycleAsync(id);
            return Ok(new { message = "Motorcycle successfully deleted" });
        }
    }
}
