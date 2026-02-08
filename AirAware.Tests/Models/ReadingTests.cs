using AirAware.Models;
using Xunit;

namespace AirAware.Tests.Models;

public class ReadingTests
{
    [Fact]
    public void Reading_DefaultValues_AreSetCorrectly()
    {
        // Arrange
        var stationId = Guid.NewGuid();

        // Act
        var reading = new Reading
        {
            StationId = stationId,
            Pm25 = 12.5
        };

        // Assert
        Assert.NotEqual(Guid.Empty, reading.Id);
        Assert.Equal(stationId, reading.StationId);
        Assert.True(reading.CreatedAt <= DateTime.Now);
        Assert.True(reading.CreatedAt >= DateTime.Now.AddSeconds(-1));
    }

    [Fact]
    public void Reading_WithAllFields_StoresCorrectly()
    {
        // Arrange
        var stationId = Guid.NewGuid();
        var rawPayload = "{\"sensor\":\"BME680\",\"temperature\":22.5}";

        // Act
        var reading = new Reading
        {
            StationId = stationId,
            Pm25 = 35.5,
            Pm10 = 154.0,
            RawPayload = rawPayload
        };

        // Assert
        Assert.Equal(stationId, reading.StationId);
        Assert.Equal(35.5, reading.Pm25);
        Assert.Equal(154.0, reading.Pm10);
        Assert.Equal(rawPayload, reading.RawPayload);
    }

    [Fact]
    public void Reading_Pm10CanBeNull()
    {
        // Arrange
        var stationId = Guid.NewGuid();

        // Act
        var reading = new Reading
        {
            StationId = stationId,
            Pm25 = 12.5,
            Pm10 = null
        };

        // Assert
        Assert.Null(reading.Pm10);
        Assert.Equal(12.5, reading.Pm25);
    }

    [Fact]
    public void Reading_RawPayloadCanBeNull()
    {
        // Arrange
        var stationId = Guid.NewGuid();

        // Act
        var reading = new Reading
        {
            StationId = stationId,
            Pm25 = 12.5,
            RawPayload = null
        };

        // Assert
        Assert.Null(reading.RawPayload);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(12.5)]
    [InlineData(35.4)]
    [InlineData(150.5)]
    [InlineData(500.0)]
    public void Reading_Pm25Values_AreStoredCorrectly(double pm25Value)
    {
        // Arrange
        var stationId = Guid.NewGuid();

        // Act
        var reading = new Reading
        {
            StationId = stationId,
            Pm25 = pm25Value
        };

        // Assert
        Assert.Equal(pm25Value, reading.Pm25);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(50.0)]
    [InlineData(154.0)]
    [InlineData(354.0)]
    [InlineData(504.0)]
    public void Reading_Pm10Values_AreStoredCorrectly(double pm10Value)
    {
        // Arrange
        var stationId = Guid.NewGuid();

        // Act
        var reading = new Reading
        {
            StationId = stationId,
            Pm25 = 12.5,
            Pm10 = pm10Value
        };

        // Assert
        Assert.Equal(pm10Value, reading.Pm10);
    }

    [Fact]
    public void Reading_UniqueIds_AreGeneratedForDifferentInstances()
    {
        // Arrange
        var stationId = Guid.NewGuid();

        // Act
        var reading1 = new Reading { StationId = stationId, Pm25 = 12.5 };
        var reading2 = new Reading { StationId = stationId, Pm25 = 35.0 };

        // Assert
        Assert.NotEqual(reading1.Id, reading2.Id);
    }
}

