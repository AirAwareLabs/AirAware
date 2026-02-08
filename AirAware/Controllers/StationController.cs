using AirAware.Data;
using AirAware.Models;
using AirAware.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirAware.Controllers;

[ApiController]
[Route("api/v1")]
public class StationController: ControllerBase
{
    [HttpGet]
    [Route("stations")]
    public async Task<IActionResult> GetAsync([FromServices] AppDbContext context)
    {
        var stations = await context
            .Stations
            .AsNoTracking()
            .ToListAsync();
        return Ok(stations);
    }
    
    [HttpGet]
    [Route("stations/{id}")]
    public async Task<IActionResult> GetByIdAsync(
        [FromServices] AppDbContext context, 
        [FromRoute] Guid id
    )
    {
        var station = await context
            .Stations
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
        
        return station == null 
            ? NotFound() 
            : Ok(station);
    }
    
    [HttpPost("stations")]
    public async Task<IActionResult> PostAsync(
        [FromServices] AppDbContext context,
        [FromBody] CreateStationViewModel model
    )
    {
        if (!ModelState.IsValid) 
            return BadRequest();

        var station = new Station
        {
            Name = model.Name,
            Latitude = model.Latitude!.Value,
            Longitude = model.Longitude!.Value,
            Provider = model.Provider,
            Metadata = model.Metadata
        };

        try
        {
            await context.Stations.AddAsync(station);
            await context.SaveChangesAsync();
            return Created($"v1/stations/{station.Id}", station);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
    
    [HttpPut]
    [Route("stations/{id}")]
    public async Task<IActionResult> PutAsync(
        [FromServices] AppDbContext context, 
        [FromBody] CreateStationViewModel model,
        [FromRoute] Guid id
    )
    {
        if (!ModelState.IsValid) 
            return BadRequest();
        
        var station = await context
            .Stations
            .FirstOrDefaultAsync(s => s.Id == id);
        
        if (station == null)
            return NotFound();

        try
        {
            station.Name = model.Name;
            station.Latitude = model.Latitude!.Value;
            station.Longitude = model.Longitude!.Value;
            station.Provider = model.Provider;
            station.Metadata = model.Metadata;
            
            context.Stations.Update(station);
            await context.SaveChangesAsync();

            return Ok(station);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}