using AirAware.Data;
using AirAware.Models;
using AirAware.ViewModels;
using AirAware.Utils;
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
        Logger.LogStationOperation(_logger, "GetAllStations");
        var stations = await context
            .Stations
            .AsNoTracking()
            .ToListAsync();
        Logger.LogStationOperation(_logger, "GetAllStations", details: $"Retrieved {stations.Count} stations");
        return Ok(stations);
    }
    
    [HttpGet]
    [Route("stations/{id}")]
    public async Task<IActionResult> GetByIdAsync(
        [FromServices] AppDbContext context, 
        [FromRoute] Guid id
    )
    {
        Logger.LogStationOperation(_logger, "GetStationById", id);
        var station = await context
            .Stations
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
        
        if (station == null)
        {
            Logger.LogWarning(_logger, "GetStationById", $"Station not found: {id}");
            return NotFound();
        }
        
        return Ok(station);
    }
    
    [HttpPost("stations")]
    public async Task<IActionResult> PostAsync(
        [FromServices] AppDbContext context,
        [FromBody] CreateStationViewModel model
    )
    {
        if (!ModelState.IsValid)
        {
            Logger.LogWarning(_logger, "CreateStation", "Invalid model state");
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
            Logger.LogStationOperation(_logger, "CreateStation", station.Id, $"Created station: {station.Name}");
            return Created($"api/v1/stations/{station.Id}", station);
        }
        catch (Exception ex)
        {
            Logger.LogError(_logger, "CreateStation", ex);
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
        if (!ModelState.IsValid)
        {
            Logger.LogWarning(_logger, "UpdateStation", "Invalid model state");
            return BadRequest();
        }
        
        Logger.LogStationOperation(_logger, "UpdateStation", id);
        var station = await context
            .Stations
            .FirstOrDefaultAsync(s => s.Id == id);
        
        if (station == null)
        {
            Logger.LogWarning(_logger, "UpdateStation", $"Station not found: {id}");
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
            Logger.LogStationOperation(_logger, "UpdateStation", id, "Station updated successfully");

            return Ok(station);
        }
        catch (Exception ex)
        {
            Logger.LogError(_logger, "UpdateStation", ex);
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
        Logger.LogStationOperation(_logger, "GetLatestAqi", id);
        
        // Ensure station exists
        var station = await context.Stations.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (station == null)
        {
            Logger.LogWarning(_logger, "GetLatestAqi", $"Station not found: {id}");
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
            Logger.LogWarning(_logger, "GetLatestAqi", $"No AQI records found for station: {id}");
            return NotFound("No AQI records for station.");
        }

        Logger.LogStationOperation(_logger, "GetLatestAqi", id, $"Latest AQI: {latest.AqiValue} ({latest.Category})");
        return Ok(latest);
    }
}