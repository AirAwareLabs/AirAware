using AirAware.Controllers;
using AirAware.Data;
using AirAware.Models;
using AirAware.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AirAware.Tests.Controllers;

public class StationControllerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly StationController _controller;

    public StationControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _controller = new StationController();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAsync_ReturnsAllStations()
    {
        // Arrange
        var station1 = new Station { Name = "Station 1", Latitude = 40.7128, Longitude = -74.0060 };
        var station2 = new Station { Name = "Station 2", Latitude = 34.0522, Longitude = -118.2437 };
        await _context.Stations.AddRangeAsync(station1, station2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAsync(_context);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var stations = Assert.IsAssignableFrom<List<Station>>(okResult.Value);
        Assert.Equal(2, stations.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsStation()
    {
        // Arrange
        var station = new Station { Name = "Test Station", Latitude = 40.7128, Longitude = -74.0060 };
        await _context.Stations.AddAsync(station);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetByIdAsync(_context, station.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedStation = Assert.IsType<Station>(okResult.Value);
        Assert.Equal(station.Id, returnedStation.Id);
        Assert.Equal("Test Station", returnedStation.Name);
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
    public async Task PostAsync_ValidModel_CreatesStation()
    {
        // Arrange
        var model = new CreateStationViewModel
        {
            Name = "New Station",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Provider = "Test Provider"
        };

        // Act
        var result = await _controller.PostAsync(_context, model);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result);
        var station = Assert.IsType<Station>(createdResult.Value);
        Assert.Equal("New Station", station.Name);
        Assert.Equal(40.7128, station.Latitude);
        Assert.Equal(-74.0060, station.Longitude);
        Assert.Equal("Test Provider", station.Provider);
        Assert.True(station.Active);

        // Verify it was saved to the database
        var savedStation = await _context.Stations.FindAsync(station.Id);
        Assert.NotNull(savedStation);
        Assert.Equal("New Station", savedStation.Name);
    }

    [Fact]
    public async Task PostAsync_WithMetadata_CreatesStationWithMetadata()
    {
        // Arrange
        var model = new CreateStationViewModel
        {
            Name = "Station with Metadata",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Metadata = "{\"sensorType\":\"optical\",\"location\":\"outdoor\"}"
        };

        // Act
        var result = await _controller.PostAsync(_context, model);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result);
        var station = Assert.IsType<Station>(createdResult.Value);
        Assert.Equal("{\"sensorType\":\"optical\",\"location\":\"outdoor\"}", station.Metadata);
    }

    [Fact]
    public async Task PutAsync_ExistingStation_UpdatesFields()
    {
        // Arrange
        var station = new Station 
        { 
            Name = "Original Name", 
            Latitude = 40.7128, 
            Longitude = -74.0060,
            Provider = "Original Provider"
        };
        await _context.Stations.AddAsync(station);
        await _context.SaveChangesAsync();

        var updateModel = new UpdateStationViewModel
        {
            Name = "Updated Name",
            Latitude = 34.0522,
            Longitude = -118.2437,
            Provider = "Updated Provider"
        };

        // Act
        var result = await _controller.PutAsync(_context, updateModel, station.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var updatedStation = Assert.IsType<Station>(okResult.Value);
        Assert.Equal("Updated Name", updatedStation.Name);
        Assert.Equal(34.0522, updatedStation.Latitude);
        Assert.Equal(-118.2437, updatedStation.Longitude);
        Assert.Equal("Updated Provider", updatedStation.Provider);
    }

    [Fact]
    public async Task PutAsync_PartialUpdate_OnlyUpdatesProvidedFields()
    {
        // Arrange
        var station = new Station 
        { 
            Name = "Original Name", 
            Latitude = 40.7128, 
            Longitude = -74.0060,
            Provider = "Original Provider",
            Active = true
        };
        await _context.Stations.AddAsync(station);
        await _context.SaveChangesAsync();

        var updateModel = new UpdateStationViewModel
        {
            Name = "Updated Name"
            // Only updating name, other fields are null
        };

        // Act
        var result = await _controller.PutAsync(_context, updateModel, station.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var updatedStation = Assert.IsType<Station>(okResult.Value);
        Assert.Equal("Updated Name", updatedStation.Name);
        // Other fields should remain unchanged
        Assert.Equal(40.7128, updatedStation.Latitude);
        Assert.Equal(-74.0060, updatedStation.Longitude);
        Assert.Equal("Original Provider", updatedStation.Provider);
        Assert.True(updatedStation.Active);
    }

    [Fact]
    public async Task PutAsync_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        var updateModel = new UpdateStationViewModel
        {
            Name = "Updated Name"
        };

        // Act
        var result = await _controller.PutAsync(_context, updateModel, nonExistingId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task PostAsync_CreatesStationWithTimestamp()
    {
        // Arrange
        var model = new CreateStationViewModel
        {
            Name = "Timestamped Station",
            Latitude = 40.7128,
            Longitude = -74.0060
        };

        // Act
        var result = await _controller.PostAsync(_context, model);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result);
        var station = Assert.IsType<Station>(createdResult.Value);
        Assert.True(station.CreatedAt <= DateTime.Now);
        Assert.True(station.CreatedAt >= DateTime.Now.AddSeconds(-5));
    }
}


