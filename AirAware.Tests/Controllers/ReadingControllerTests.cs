using AirAware.Controllers;
using AirAware.Data;
using AirAware.Models;
using AirAware.Services;
using AirAware.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AirAware.Tests.Controllers;

public class ReadingControllerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ReadingController _controller;
    private readonly Mock<IAqiCalculator> _mockCalculator;

    public ReadingControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _controller = new ReadingController();
        _mockCalculator = new Mock<IAqiCalculator>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAsync_ReturnsAllReadings()
    {
        // Arrange
        var station = new Station { Name = "Test Station", Latitude = 40.7128, Longitude = -74.0060 };
        await _context.Stations.AddAsync(station);
        await _context.SaveChangesAsync();

        var reading1 = new Reading { StationId = station.Id, Pm25 = 12.0, Pm10 = 50 };
        var reading2 = new Reading { StationId = station.Id, Pm25 = 35.0, Pm10 = 100 };
        await _context.Readings.AddRangeAsync(reading1, reading2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAsync(_context);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var readings = Assert.IsAssignableFrom<List<Reading>>(okResult.Value);
        Assert.Equal(2, readings.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsReading()
    {
        // Arrange
        var station = new Station { Name = "Test Station", Latitude = 40.7128, Longitude = -74.0060 };
        await _context.Stations.AddAsync(station);
        await _context.SaveChangesAsync();

        var reading = new Reading { StationId = station.Id, Pm25 = 12.0, Pm10 = 50 };
        await _context.Readings.AddAsync(reading);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetByIdAsync(_context, reading.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedReading = Assert.IsType<Reading>(okResult.Value);
        Assert.Equal(reading.Id, returnedReading.Id);
        Assert.Equal(12.0, returnedReading.Pm25);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _controller.GetByIdAsync(_context, nonExistingId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task PostAsync_ValidModel_CreatesReadingAndAqiRecord()
    {
        // Arrange
        var station = new Station { Name = "Test Station", Latitude = 40.7128, Longitude = -74.0060 };
        await _context.Stations.AddAsync(station);
        await _context.SaveChangesAsync();

        var model = new CreateReadingViewModel
        {
            StationId = station.Id,
            Pm25 = 35.5,
            Pm10 = 154
        };

        // Setup mock calculator
        var pm25Result = new AqiResult(101, "Unhealthy for Sensitive Groups", "PM2.5");
        var pm10Result = new AqiResult(100, "Moderate", "PM10");
        var finalResult = pm25Result;

        _mockCalculator
            .Setup(c => c.Calculate(It.IsAny<Reading>()))
            .Returns((finalResult, pm25Result, pm10Result));

        // Act
        var result = await _controller.PostAsync(_context, _mockCalculator.Object, model);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result);
        Assert.NotNull(createdResult.Value);
        
        // Verify reading was created
        var readingCount = await _context.Readings.CountAsync();
        Assert.Equal(1, readingCount);

        // Verify AQI record was created
        var aqiRecordCount = await _context.AqiRecords.CountAsync();
        Assert.Equal(1, aqiRecordCount);

        var aqiRecord = await _context.AqiRecords.FirstAsync();
        Assert.Equal(101, aqiRecord.AqiValue);
        Assert.Equal("Unhealthy for Sensitive Groups", aqiRecord.Category);
        Assert.Equal(101, aqiRecord.Pm25Aqi);
        Assert.Equal(100, aqiRecord.Pm10Aqi);
    }

    [Fact]
    public async Task PostAsync_NonExistingStation_ReturnsBadRequest()
    {
        // Arrange
        var nonExistingStationId = Guid.NewGuid();
        var model = new CreateReadingViewModel
        {
            StationId = nonExistingStationId,
            Pm25 = 12.0,
            Pm10 = 50
        };

        // Act
        var result = await _controller.PostAsync(_context, _mockCalculator.Object, model);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Station with the provided ID does not exist.", badRequestResult.Value);
    }

    [Fact]
    public async Task PostAsync_WithRawPayload_StoresRawPayload()
    {
        // Arrange
        var station = new Station { Name = "Test Station", Latitude = 40.7128, Longitude = -74.0060 };
        await _context.Stations.AddAsync(station);
        await _context.SaveChangesAsync();

        var rawPayload = "{\"sensor\":\"BME680\",\"pm25\":12.5,\"pm10\":50}";
        var model = new CreateReadingViewModel
        {
            StationId = station.Id,
            Pm25 = 12.5,
            Pm10 = 50,
            RawPayload = rawPayload
        };

        _mockCalculator
            .Setup(c => c.Calculate(It.IsAny<Reading>()))
            .Returns((new AqiResult(50, "Good", "PM2.5"), new AqiResult(50, "Good", "PM2.5"), new AqiResult(50, "Good", "PM10")));

        // Act
        var result = await _controller.PostAsync(_context, _mockCalculator.Object, model);

        // Assert
        Assert.IsType<CreatedResult>(result);
        var reading = await _context.Readings.FirstAsync();
        Assert.Equal(rawPayload, reading.RawPayload);
    }

    [Fact]
    public async Task PostAsync_WithRawPayloadAndMissingPm10_ExtractsPm10FromPayload()
    {
        // Arrange
        var station = new Station { Name = "Test Station", Latitude = 40.7128, Longitude = -74.0060 };
        await _context.Stations.AddAsync(station);
        await _context.SaveChangesAsync();

        var rawPayload = "{\"pm25\":12.5,\"pm10\":75}";
        var model = new CreateReadingViewModel
        {
            StationId = station.Id,
            Pm25 = 12.5,
            Pm10 = null,
            RawPayload = rawPayload
        };

        _mockCalculator
            .Setup(c => c.Calculate(It.IsAny<Reading>()))
            .Returns((new AqiResult(50, "Good", "PM2.5"), new AqiResult(50, "Good", "PM2.5"), new AqiResult(50, "Good", "PM10")));

        // Act
        var result = await _controller.PostAsync(_context, _mockCalculator.Object, model);

        // Assert
        Assert.IsType<CreatedResult>(result);
        var reading = await _context.Readings.FirstAsync();
        Assert.Equal(75, reading.Pm10);
    }

    [Fact]
    public async Task PostAsync_ExtractsAlternativePm10Fields()
    {
        // Arrange
        var station = new Station { Name = "Test Station", Latitude = 40.7128, Longitude = -74.0060 };
        await _context.Stations.AddAsync(station);
        await _context.SaveChangesAsync();

        // Test pm_10 field (underscore variant)
        var rawPayload = "{\"pm25\":12.5,\"pm_10\":80}";
        var model = new CreateReadingViewModel
        {
            StationId = station.Id,
            Pm25 = 12.5,
            Pm10 = null,
            RawPayload = rawPayload
        };

        _mockCalculator
            .Setup(c => c.Calculate(It.IsAny<Reading>()))
            .Returns((new AqiResult(50, "Good", "PM2.5"), new AqiResult(50, "Good", "PM2.5"), new AqiResult(50, "Good", "PM10")));

        // Act
        var result = await _controller.PostAsync(_context, _mockCalculator.Object, model);

        // Assert
        Assert.IsType<CreatedResult>(result);
        var reading = await _context.Readings.FirstAsync();
        Assert.Equal(80, reading.Pm10);
    }

    [Fact]
    public async Task PostAsync_CreatesReadingWithTimestamp()
    {
        // Arrange
        var station = new Station { Name = "Test Station", Latitude = 40.7128, Longitude = -74.0060 };
        await _context.Stations.AddAsync(station);
        await _context.SaveChangesAsync();

        var model = new CreateReadingViewModel
        {
            StationId = station.Id,
            Pm25 = 12.0,
            Pm10 = 50
        };

        _mockCalculator
            .Setup(c => c.Calculate(It.IsAny<Reading>()))
            .Returns((new AqiResult(50, "Good", "PM2.5"), new AqiResult(50, "Good", "PM2.5"), new AqiResult(50, "Good", "PM10")));

        // Act
        var result = await _controller.PostAsync(_context, _mockCalculator.Object, model);

        // Assert
        Assert.IsType<CreatedResult>(result);
        var reading = await _context.Readings.FirstAsync();
        Assert.True(reading.CreatedAt <= DateTime.Now);
        Assert.True(reading.CreatedAt >= DateTime.Now.AddSeconds(-5));
    }

    [Fact]
    public async Task PostAsync_DoesNotCreateDuplicateAqiRecords()
    {
        // Arrange
        var station = new Station { Name = "Test Station", Latitude = 40.7128, Longitude = -74.0060 };
        await _context.Stations.AddAsync(station);
        await _context.SaveChangesAsync();

        var reading = new Reading { StationId = station.Id, Pm25 = 12.0, Pm10 = 50 };
        await _context.Readings.AddAsync(reading);
        await _context.SaveChangesAsync();

        // Create an existing AQI record
        var existingAqiRecord = new AqiRecord
        {
            ReadingId = reading.Id,
            StationId = station.Id,
            AqiValue = 50,
            Category = "Good"
        };
        await _context.AqiRecords.AddAsync(existingAqiRecord);
        await _context.SaveChangesAsync();

        var model = new CreateReadingViewModel
        {
            StationId = station.Id,
            Pm25 = 35.0,
            Pm10 = 100
        };

        _mockCalculator
            .Setup(c => c.Calculate(It.IsAny<Reading>()))
            .Returns((new AqiResult(100, "Moderate", "PM2.5"), new AqiResult(100, "Moderate", "PM2.5"), new AqiResult(100, "Moderate", "PM10")));

        // Act
        await _controller.PostAsync(_context, _mockCalculator.Object, model);

        // Assert - Should still have only 2 AQI records (1 existing + 1 new)
        var aqiRecordCount = await _context.AqiRecords.CountAsync();
        Assert.Equal(2, aqiRecordCount);
    }
}






