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
    private readonly ILogger<StationController> _logger;
    
    public StationController(ILogger<StationController> logger)
    {
        _logger = logger;
    }
    
    [HttpGet]
    [Route("stations")]
    public async Task<IActionResult> GetAsync([FromServices] AppDbContext context)
    {
        _logger.LogInformation("Fetching all stations");
        
        var stations = await context
            .Stations
            .AsNoTracking()
            .ToListAsync();
        
        _logger.LogInformation("Retrieved {Count} stations", stations.Count);
        return Ok(stations);
    }
    
    [HttpGet]
    [Route("stations/{id}")]
    public async Task<IActionResult> GetByIdAsync(
        [FromServices] AppDbContext context, 
        [FromRoute] Guid id
    )
    {
        _logger.LogInformation("Fetching station with ID: {StationId}", id);
        
        var station = await context
            .Stations
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
        
        if (station == null)
        {
            _logger.LogWarning("Station with ID {StationId} not found", id);
            return NotFound();
        }
        
        _logger.LogInformation("Successfully retrieved station with ID: {StationId}", id);
        return Ok(station);
    }
    
    [HttpPost("stations")]
    public async Task<IActionResult> PostAsync(
        [FromServices] AppDbContext context,
        [FromBody] CreateStationViewModel model
    )
    {
        _logger.LogInformation("Creating new station: {StationName}", model.Name);
        
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for station creation");
            return BadRequest();
        }

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
            
            _logger.LogInformation("Station {StationId} created successfully: {StationName}", station.Id, station.Name);
            return Created($"api/v1/stations/{station.Id}", station);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating station: {StationName}", model.Name);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
    
    [HttpPut]
    [Route("stations/{id}")]
    public async Task<IActionResult> PutAsync(
        [FromServices] AppDbContext context, 
        [FromBody] UpdateStationViewModel model,
        [FromRoute] Guid id
    )
    {
        _logger.LogInformation("Updating station with ID: {StationId}", id);
        
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for station update");
            return BadRequest();
        }
        
        var station = await context
            .Stations
            .FirstOrDefaultAsync(s => s.Id == id);
        
        if (station == null)
        {
            _logger.LogWarning("Station with ID {StationId} not found for update", id);
            return NotFound();
        }

        try
        {
            if (model.Name != null)
                station.Name = model.Name;
            if (model.Latitude != null)
                station.Latitude = model.Latitude.Value;
            if (model.Longitude != null)
                station.Longitude = model.Longitude.Value;
            if (model.Provider != null)
                station.Provider = model.Provider;
            if (model.Metadata != null)
                station.Metadata = model.Metadata;
            if (model.Active != null)
                station.Active = model.Active.Value;
            
            await context.SaveChangesAsync();

            _logger.LogInformation("Station {StationId} updated successfully", id);
            return Ok(station);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating station {StationId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
    
    [HttpGet]
    [Route("stations/{id}/aqi/latest")]
    public async Task<IActionResult> GetLatestAqiForStation(
        [FromServices] AppDbContext context,
        [FromRoute] Guid id
    )
    {
        _logger.LogInformation("Fetching latest AQI for station {StationId}", id);
        
        // Ensure station exists
        var station = await context.Stations.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (station == null)
        {
            _logger.LogWarning("Station {StationId} not found", id);
            return NotFound("Station not found.");
        }

        // Get latest AQI record for this station, including its reading
        var latest = await context.AqiRecords
            .AsNoTracking()
            .Where(a => a.StationId == id)
            .OrderByDescending(a => a.ComputedAt)
            .Join(context.Readings.AsNoTracking(),
                a => a.ReadingId,
                r => r.Id,
                (a, r) => new { Aqi = a, Reading = r })
            .Select(ar => new
            {
                ar.Aqi.Id,
                ar.Aqi.AqiValue,
                ar.Aqi.Category,
                ar.Aqi.ComputedAt,
                Reading = new { ar.Reading.Id, ar.Reading.Pm25, ar.Reading.Pm10, ar.Reading.CreatedAt }
            })
            .FirstOrDefaultAsync();

        if (latest == null)
        {
            _logger.LogWarning("No AQI records found for station {StationId}", id);
            return NotFound("No AQI records for station.");
        }

        _logger.LogInformation("Retrieved latest AQI for station {StationId}: {AqiValue} ({Category})", 
            id, latest.AqiValue, latest.Category);
        return Ok(latest);
    }
}