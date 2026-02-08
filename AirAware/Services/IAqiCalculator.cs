using AirAware.Models;

namespace AirAware.Services;

public interface IAqiCalculator
{
    /// <summary>
    /// Calculates AQI for a reading. Returns the selected final AQI (higher of PM2.5 or PM10),
    /// and the individual results for PM2.5 and PM10.
    /// </summary>
    (AqiResult final, AqiResult pm25, AqiResult pm10) Calculate(Reading reading);

    AqiResult CalculateForPm25(double concentration);
    AqiResult CalculateForPm10(double concentration);
}

public record AqiResult(int Value, string Category, string Pollutant);