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
        [FromServices] IAqiCalculator aqiCalculator,
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

            // compute AQI synchronously
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
            }

            return Created($"api/v1/readings/{reading.Id}", new { reading, aqi = aqiRecord });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}