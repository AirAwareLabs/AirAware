using AirAware.Models;
using Xunit;

namespace AirAware.Tests.Models;

public class StationTests
{
    [Fact]
    public void Station_DefaultValues_AreSetCorrectly()
    {
        // Act
        var before = DateTime.Now;
        var station = new Station
        {
            Name = "Test Station",
            Latitude = 40.7128,
            Longitude = -74.0060
        };
        var after = DateTime.Now;
        
        // Assert
        Assert.NotEqual(Guid.Empty, station.Id);
        Assert.True(station.Active);
        Assert.True(station.CreatedAt >= before);
        Assert.True(station.CreatedAt <= after);
    }

    [Fact]
    public void Station_WithAllFields_StoresCorrectly()
    {
        // Arrange & Act
        var station = new Station
        {
            Name = "Full Station",
            Latitude = 40.7128,
            Longitude = -74.0060,
            Provider = "Test Provider",
            Metadata = "{\"type\":\"outdoor\"}",
            Active = false
        };

        // Assert
        Assert.Equal("Full Station", station.Name);
        Assert.Equal(40.7128, station.Latitude);
        Assert.Equal(-74.0060, station.Longitude);
        Assert.Equal("Test Provider", station.Provider);
        Assert.Equal("{\"type\":\"outdoor\"}", station.Metadata);
        Assert.False(station.Active);
    }

    [Theory]
    [InlineData(90.0, 180.0)]
    [InlineData(-90.0, -180.0)]
    [InlineData(0.0, 0.0)]
    [InlineData(45.5, -122.6)]
    public void Station_ValidCoordinates_AreAccepted(double latitude, double longitude)
    {
        // Act
        var station = new Station
        {
            Name = "Coordinate Test",
            Latitude = latitude,
            Longitude = longitude
        };

        // Assert
        Assert.Equal(latitude, station.Latitude);
        Assert.Equal(longitude, station.Longitude);
    }

    [Fact]
    public void Station_UniqueIds_AreGeneratedForDifferentInstances()
    {
        // Act
        var station1 = new Station { Name = "Station 1", Latitude = 40.7128, Longitude = -74.0060 };
        var station2 = new Station { Name = "Station 2", Latitude = 34.0522, Longitude = -118.2437 };

        // Assert
        Assert.NotEqual(station1.Id, station2.Id);
    }
}

