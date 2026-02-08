using AirAware.Data;
using AirAware.Models;
using AirAware.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirAware.Controllers;

[ApiController]
[Route("api/v1")]
public class ReadingController: ControllerBase
{
    [HttpGet]
    [Route("readings")]
    public async Task<IActionResult> GetAsync([FromServices] AppDbContext context)
    {
        var readings = await context
            .Readings
            .AsNoTracking()
            .ToListAsync();
        return Ok(readings);
    }
    
    [HttpGet]
    [Route("readings/{id}")]
    public async Task<IActionResult> GetByIdAsync(
        [FromServices] AppDbContext context, 
        [FromRoute] Guid id
    )
    {
        var reading = await context
            .Readings
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
        
        return reading == null 
            ? NotFound() 
            : Ok(reading);
    }
    
    [HttpPost("readings")]
    public async Task<IActionResult> PostAsync(
        [FromServices] AppDbContext context,
        [FromBody] CreateReadingViewModel model
    )
    {
        if (!ModelState.IsValid) 
            return BadRequest("Invalid data provided.");
        
        var station = await context
            .Stations
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == model.StationId);
        
        if (station == null) 
            return BadRequest("Station with the provided ID does not exist.");

        var reading = new Reading
        {
            StationId = model.StationId,
            Pm2_5 = model.Pm2_5,
            Pm10 = model.Pm10,
            RawPayload = model.RawPayload
        };

        try
        {
            await context.Readings.AddAsync(reading);
            await context.SaveChangesAsync();
            return Created($"api/v1/readings/{reading.Id}", reading);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}