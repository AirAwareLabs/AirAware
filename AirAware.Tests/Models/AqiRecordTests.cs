using AirAware.Models;
using Xunit;

namespace AirAware.Tests.Models;

public class AqiRecordTests
{
    [Fact]
    public void AqiRecord_DefaultValues_AreSetCorrectly()
    {
        // Act
        var aqiRecord = new AqiRecord
        {
            ReadingId = Guid.NewGuid(),
            StationId = Guid.NewGuid(),
            AqiValue = 50,
            Category = "Good"
        };

        // Assert
        Assert.NotEqual(Guid.Empty, aqiRecord.Id);
        Assert.True(aqiRecord.ComputedAt <= DateTime.Now);
        Assert.True(aqiRecord.ComputedAt >= DateTime.Now.AddSeconds(-1));
    }

    [Fact]
    public void AqiRecord_WithAllFields_StoresCorrectly()
    {
        // Arrange
        var readingId = Guid.NewGuid();
        var stationId = Guid.NewGuid();

        // Act
        var aqiRecord = new AqiRecord
        {
            ReadingId = readingId,
            StationId = stationId,
            AqiValue = 101,
            Category = "Unhealthy for Sensitive Groups",
            Pm25Aqi = 101,
            Pm10Aqi = 100,
            Pm25Category = "Unhealthy for Sensitive Groups",
            Pm10Category = "Moderate"
        };

        // Assert
        Assert.Equal(readingId, aqiRecord.ReadingId);
        Assert.Equal(stationId, aqiRecord.StationId);
        Assert.Equal(101, aqiRecord.AqiValue);
        Assert.Equal("Unhealthy for Sensitive Groups", aqiRecord.Category);
        Assert.Equal(101, aqiRecord.Pm25Aqi);
        Assert.Equal(100, aqiRecord.Pm10Aqi);
        Assert.Equal("Unhealthy for Sensitive Groups", aqiRecord.Pm25Category);
        Assert.Equal("Moderate", aqiRecord.Pm10Category);
    }

    [Fact]
    public void AqiRecord_Pm25AndPm10CanBeNull()
    {
        // Act
        var aqiRecord = new AqiRecord
        {
            ReadingId = Guid.NewGuid(),
            StationId = Guid.NewGuid(),
            AqiValue = 50,
            Category = "Good",
            Pm25Aqi = null,
            Pm10Aqi = null,
            Pm25Category = null,
            Pm10Category = null
        };

        // Assert
        Assert.Null(aqiRecord.Pm25Aqi);
        Assert.Null(aqiRecord.Pm10Aqi);
        Assert.Null(aqiRecord.Pm25Category);
        Assert.Null(aqiRecord.Pm10Category);
    }

    [Theory]
    [InlineData(0, "Good")]
    [InlineData(50, "Good")]
    [InlineData(51, "Moderate")]
    [InlineData(100, "Moderate")]
    [InlineData(101, "Unhealthy for Sensitive Groups")]
    [InlineData(150, "Unhealthy for Sensitive Groups")]
    [InlineData(151, "Unhealthy")]
    [InlineData(200, "Unhealthy")]
    [InlineData(201, "Very Unhealthy")]
    [InlineData(300, "Very Unhealthy")]
    [InlineData(301, "Hazardous")]
    [InlineData(500, "Hazardous")]
    public void AqiRecord_AqiValuesAndCategories_AreStoredCorrectly(int aqiValue, string category)
    {
        // Act
        var aqiRecord = new AqiRecord
        {
            ReadingId = Guid.NewGuid(),
            StationId = Guid.NewGuid(),
            AqiValue = aqiValue,
            Category = category
        };

        // Assert
        Assert.Equal(aqiValue, aqiRecord.AqiValue);
        Assert.Equal(category, aqiRecord.Category);
    }

    [Fact]
    public void AqiRecord_UniqueIds_AreGeneratedForDifferentInstances()
    {
        // Arrange
        var readingId = Guid.NewGuid();
        var stationId = Guid.NewGuid();

        // Act
        var aqiRecord1 = new AqiRecord
        {
            ReadingId = readingId,
            StationId = stationId,
            AqiValue = 50,
            Category = "Good"
        };

        var aqiRecord2 = new AqiRecord
        {
            ReadingId = readingId,
            StationId = stationId,
            AqiValue = 100,
            Category = "Moderate"
        };

        // Assert
        Assert.NotEqual(aqiRecord1.Id, aqiRecord2.Id);
    }

    [Fact]
    public void AqiRecord_TracksReadingAndStation()
    {
        // Arrange
        var readingId = Guid.NewGuid();
        var stationId = Guid.NewGuid();

        // Act
        var aqiRecord = new AqiRecord
        {
            ReadingId = readingId,
            StationId = stationId,
            AqiValue = 75,
            Category = "Moderate"
        };

        // Assert
        Assert.Equal(readingId, aqiRecord.ReadingId);
        Assert.Equal(stationId, aqiRecord.StationId);
    }
}


