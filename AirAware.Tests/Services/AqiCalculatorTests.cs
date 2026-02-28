using AirAware.Models;
using AirAware.Services;
using Xunit;

namespace AirAware.Tests.Services;

public class AqiCalculatorTests
{
    private readonly AqiCalculator _calculator;

    public AqiCalculatorTests()
    {
        _calculator = new AqiCalculator();
    }

    [Theory]
    [InlineData(0.0, 0, "Good")]
    [InlineData(12.0, 50, "Good")]
    [InlineData(12.1, 51, "Moderate")]
    [InlineData(35.4, 100, "Moderate")]
    [InlineData(35.5, 101, "Unhealthy for Sensitive Groups")]
    [InlineData(55.4, 150, "Unhealthy for Sensitive Groups")]
    [InlineData(55.5, 151, "Unhealthy")]
    [InlineData(150.4, 200, "Unhealthy")]
    [InlineData(150.5, 201, "Very Unhealthy")]
    [InlineData(250.4, 300, "Very Unhealthy")]
    [InlineData(250.5, 301, "Hazardous")]
    [InlineData(500.4, 500, "Hazardous")]
    public void CalculateForPm25_ValidConcentration_ReturnsCorrectAqi(double concentration, int expectedAqi, string expectedCategory)
    {
        // Act
        var result = _calculator.CalculateForPm25(concentration);

        // Assert
        Assert.Equal(expectedAqi, result.Value);
        Assert.Equal(expectedCategory, result.Category);
        Assert.Equal("PM2.5", result.Pollutant);
    }

    [Theory]
    [InlineData(0, 0, "Good")]
    [InlineData(54, 50, "Good")]
    [InlineData(55, 51, "Moderate")]
    [InlineData(154, 100, "Moderate")]
    [InlineData(155, 101, "Unhealthy for Sensitive Groups")]
    [InlineData(254, 150, "Unhealthy for Sensitive Groups")]
    [InlineData(255, 151, "Unhealthy")]
    [InlineData(354, 200, "Unhealthy")]
    [InlineData(355, 201, "Very Unhealthy")]
    [InlineData(424, 300, "Very Unhealthy")]
    [InlineData(425, 301, "Hazardous")]
    [InlineData(504, 500, "Hazardous")]
    public void CalculateForPm10_ValidConcentration_ReturnsCorrectAqi(double concentration, int expectedAqi, string expectedCategory)
    {
        // Act
        var result = _calculator.CalculateForPm10(concentration);

        // Assert
        Assert.Equal(expectedAqi, result.Value);
        Assert.Equal(expectedCategory, result.Category);
        Assert.Equal("PM10", result.Pollutant);
    }

    [Fact]
    public void Calculate_WithReading_ReturnsCorrectAqiForBothPollutants()
    {
        // Arrange
        var reading = new Reading
        {
            StationId = Guid.NewGuid(),
            Pm25 = 35.5, // AQI 101 - Unhealthy for Sensitive Groups
            Pm10 = 154   // AQI 100 - Moderate
        };

        // Act
        var (final, pm25, pm10) = _calculator.Calculate(reading);

        // Assert
        Assert.Equal(101, pm25.Value);
        Assert.Equal("Unhealthy for Sensitive Groups", pm25.Category);
        Assert.Equal("PM2.5", pm25.Pollutant);

        Assert.Equal(100, pm10.Value);
        Assert.Equal("Moderate", pm10.Category);
        Assert.Equal("PM10", pm10.Pollutant);

        // Final should be PM2.5 since it has higher AQI
        Assert.Equal(101, final.Value);
        Assert.Equal("Unhealthy for Sensitive Groups", final.Category);
        Assert.Equal("PM2.5", final.Pollutant);
    }

    [Fact]
    public void Calculate_WithNullPm10_UsesPm25AsFinal()
    {
        // Arrange
        var reading = new Reading
        {
            StationId = Guid.NewGuid(),
            Pm25 = 12.0,  // AQI 50 - Good
            Pm10 = null   // Will default to 0
        };

        // Act
        var (final, pm25, pm10) = _calculator.Calculate(reading);

        // Assert
        Assert.Equal(50, pm25.Value);
        Assert.Equal("Good", pm25.Category);

        Assert.Equal(0, pm10.Value);
        Assert.Equal("Good", pm10.Category);

        // Final should be PM2.5 since it has higher AQI
        Assert.Equal(50, final.Value);
        Assert.Equal("Good", final.Category);
    }

    [Fact]
    public void Calculate_Pm10HigherThanPm25_ReturnsPm10AsFinal()
    {
        // Arrange
        var reading = new Reading
        {
            StationId = Guid.NewGuid(),
            Pm25 = 12.0,  // AQI 50 - Good
            Pm10 = 354    // AQI 200 - Unhealthy
        };

        // Act
        var (final, pm25, pm10) = _calculator.Calculate(reading);

        // Assert
        Assert.Equal(50, pm25.Value);
        Assert.Equal(200, pm10.Value);

        // Final should be PM10 since it has higher AQI
        Assert.Equal(200, final.Value);
        Assert.Equal("Unhealthy", final.Category);
        Assert.Equal("PM10", final.Pollutant);
    }

    [Fact]
    public void CalculateForPm25_InterpolatesCorrectly()
    {
        // Arrange
        // Test mid-range value: PM2.5 = 24.0 (between 12.1 and 35.4)
        // Expected: AQI = (100-51)/(35.4-12.1) * (24.0-12.1) + 51 = 49/23.3 * 11.9 + 51 ≈ 76

        // Act
        var result = _calculator.CalculateForPm25(24.0);

        // Assert
        Assert.InRange(result.Value, 75, 77); // Allow small rounding differences
        Assert.Equal("Moderate", result.Category);
    }

    [Fact]
    public void CalculateForPm10_InterpolatesCorrectly()
    {
        // Arrange
        // Test mid-range value: PM10 = 100 (between 55 and 154)
        // Expected: AQI = (100-51)/(154-55) * (100-55) + 51 = 49/99 * 45 + 51 ≈ 73

        // Act
        var result = _calculator.CalculateForPm10(100);

        // Assert
        Assert.InRange(result.Value, 72, 74); // Allow small rounding differences
        Assert.Equal("Moderate", result.Category);
    }

    [Fact]
    public void CalculateForPm25_AboveMaxBreakpoint_CapsToMaximum()
    {
        // Arrange
        var concentration = 600.0; // Above maximum breakpoint

        // Act
        var result = _calculator.CalculateForPm25(concentration);

        // Assert
        Assert.Equal(500, result.Value);
        Assert.Equal("Hazardous", result.Category);
    }

    [Fact]
    public void CalculateForPm10_AboveMaxBreakpoint_CapsToMaximum()
    {
        // Arrange
        var concentration = 600.0; // Above maximum breakpoint

        // Act
        var result = _calculator.CalculateForPm10(concentration);

        // Assert
        Assert.Equal(500, result.Value);
        Assert.Equal("Hazardous", result.Category);
    }
}

