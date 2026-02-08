using AirAware.Data;
using AirAware.Models;
using AirAware.Services;
using AirAware.ViewModels;
using AirAware.Utils;
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
        Logger.LogReadingOperation(_logger, "GetAllReadings");
        var readings = await context
            .Readings
            .AsNoTracking()
            .ToListAsync();
        Logger.LogReadingOperation(_logger, "GetAllReadings", details: $"Retrieved {readings.Count} readings");
        return Ok(readings);
    }
    
    [HttpGet]
    [Route("readings/{id}")]
    public async Task<IActionResult> GetByIdAsync(
        [FromServices] AppDbContext context, 
        [FromRoute] Guid id
    )
    {
        Logger.LogReadingOperation(_logger, "GetReadingById", id);
        var reading = await context
            .Readings
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
        
        if (reading == null)
        {
            Logger.LogWarning(_logger, "GetReadingById", $"Reading not found: {id}");
            return NotFound();
        }
        
        return Ok(reading);
    }
    
    [HttpPost("readings")]
    public async Task<IActionResult> PostAsync(
        [FromServices] AppDbContext context,
        [FromServices] IAqiCalculator aqiCalculator,
        [FromBody] CreateReadingViewModel model
    )
    {
        if (!ModelState.IsValid)
        {
            Logger.LogWarning(_logger, "CreateReading", "Invalid model state");
            return BadRequest("Invalid data provided.");
        }
        
        Logger.LogReadingOperation(_logger, "CreateReading", stationId: model.StationId, details: $"PM2.5: {model.Pm25}, PM10: {model.Pm10}");
        
        var station = await context
            .Stations
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == model.StationId);
        
        if (station == null)
        {
            Logger.LogWarning(_logger, "CreateReading", $"Station not found: {model.StationId}");
            return BadRequest("Station with the provided ID does not exist.");
        }

        double? pm10 = model.Pm10;
        if (!pm10.HasValue && !string.IsNullOrWhiteSpace(model.RawPayload))
        {
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
                    Logger.LogReadingOperation(_logger, "CreateReading", stationId: model.StationId, details: $"Extracted PM10 from payload: {pm10.Value}");
                // add provider specific paths here
            }
            catch
            {
                // parsing failed — ignore and continue (we already have pm25)
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
            Logger.LogReadingOperation(_logger, "CreateReading", reading.Id, reading.StationId, "Reading saved successfully");

            // compute AQI synchronously
            var (final, pm25Result, pm10Result) = aqiCalculator.Calculate(reading);
            Logger.LogAqiCalculation(_logger, reading.Id, final.Value, final.Category, pm25Result.Value, pm10Result.Value);

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
                Logger.LogReadingOperation(_logger, "CreateReading", reading.Id, reading.StationId, "AQI record created successfully");
            }

            return Created($"api/v1/readings/{reading.Id}", new { reading, aqi = aqiRecord });
        }
        catch (Exception ex)
        {
            Logger.LogError(_logger, "CreateReading", ex);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}