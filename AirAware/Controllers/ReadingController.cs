using AirAware.Data;
using AirAware.Models;
using AirAware.Services;
using AirAware.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AirAware.Controllers;

[ApiController]
[Route("api/v1")]
public class ReadingController: ControllerBase
{
    private readonly ILogger<ReadingController> _logger;
    
    public ReadingController(ILogger<ReadingController> logger)
    {
        _logger = logger;
    }
    
    [HttpGet]
    [Route("readings")]
    public async Task<IActionResult> GetAsync([FromServices] AppDbContext context)
    {
        _logger.LogInformation("Fetching all readings");
        
        var readings = await context
            .Readings
            .AsNoTracking()
            .ToListAsync();
        
        _logger.LogInformation("Retrieved {Count} readings", readings.Count);
        return Ok(readings);
    }
    
    [HttpGet]
    [Route("readings/{id}")]
    public async Task<IActionResult> GetByIdAsync(
        [FromServices] AppDbContext context, 
        [FromRoute] Guid id
    )
    {
        _logger.LogInformation("Fetching reading with ID: {ReadingId}", id);
        
        var reading = await context
            .Readings
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
        
        if (reading == null)
        {
            _logger.LogWarning("Reading with ID {ReadingId} not found", id);
            return NotFound();
        }
        
        _logger.LogInformation("Successfully retrieved reading with ID: {ReadingId}", id);
        return Ok(reading);
    }
    
    [HttpPost("readings")]
    public async Task<IActionResult> PostAsync(
        [FromServices] AppDbContext context,
        [FromServices] IAqiCalculator aqiCalculator,
        [FromBody] CreateReadingViewModel model
    )
    {
        _logger.LogInformation("Creating new reading for station {StationId}", model.StationId);
        
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for reading creation");
            return BadRequest("Invalid data provided.");
        }
        
        var station = await context
            .Stations
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == model.StationId);
        
        if (station == null)
        {
            _logger.LogWarning("Station with ID {StationId} does not exist", model.StationId);
            return BadRequest("Station with the provided ID does not exist.");
        }

        double? pm10 = model.Pm10;
        if (!pm10.HasValue && !string.IsNullOrWhiteSpace(model.RawPayload))
        {
            _logger.LogDebug("Attempting to extract PM10 from raw payload");
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(model.RawPayload);
                var root = doc.RootElement;

                if (root.TryGetProperty("pm10", out var p10) && p10.TryGetDouble(out var val10))
                    pm10 = val10;
                else if (root.TryGetProperty("pm_10", out var p10b) && p10b.TryGetDouble(out var val10b))
                    pm10 = val10b;
                else if (root.TryGetProperty("pm10_atm", out var p10c) && p10c.TryGetDouble(out var val10c))
                    pm10 = val10c;
                
                if (pm10.HasValue)
                    _logger.LogDebug("Extracted PM10 value: {Pm10}", pm10.Value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse PM10 from raw payload");
            }
        }
        
        var reading = new Reading
        {
            StationId = model.StationId,
            Pm25 = model.Pm25,
            Pm10 = pm10,
            RawPayload = model.RawPayload
        };

        try
        {
            await context.Readings.AddAsync(reading);
            await context.SaveChangesAsync();
            
            _logger.LogInformation("Reading {ReadingId} created successfully for station {StationId}", reading.Id, reading.StationId);

            // compute AQI synchronously
            _logger.LogDebug("Calculating AQI for reading {ReadingId}", reading.Id);
            var (final, pm25Result, pm10Result) = aqiCalculator.Calculate(reading);

            var aqiRecord = new AqiRecord
            {
                ReadingId = reading.Id,
                StationId = reading.StationId,
                AqiValue = final.Value,
                Category = final.Category,
                Pm25Aqi = pm25Result.Value,
                Pm25Category = pm25Result.Category,
                Pm10Aqi = pm10Result.Value,
                Pm10Category = pm10Result.Category,
                ComputedAt = DateTime.UtcNow
            };

            // Optional: ensure unique insert by checking existing AqiRecords for reading.Id
            var existing = await context.AqiRecords.FirstOrDefaultAsync(a => a.ReadingId == reading.Id);
            if (existing == null)
            {
                await context.AqiRecords.AddAsync(aqiRecord);
                await context.SaveChangesAsync();
                _logger.LogInformation("AQI record created for reading {ReadingId} with value {AqiValue} ({Category})", 
                    reading.Id, aqiRecord.AqiValue, aqiRecord.Category);
            }
            else
            {
                _logger.LogDebug("AQI record already exists for reading {ReadingId}", reading.Id);
            }

            return Created($"api/v1/readings/{reading.Id}", new { reading, aqi = aqiRecord });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reading for station {StationId}", model.StationId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}