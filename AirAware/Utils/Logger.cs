namespace AirAware.Utils;

/// <summary>
/// Centralized logging utility for consistent log formatting across the application
/// </summary>
public static class Logger
{
    /// <summary>
    /// Logs an informational message for station operations
    /// </summary>
    public static void LogStationOperation(ILogger logger, string operation, Guid? stationId = null, string? details = null)
    {
        if (stationId.HasValue)
            logger.LogInformation("Station operation: {Operation} | StationId: {StationId} | {Details}", 
                operation, stationId.Value, details ?? "");
        else
            logger.LogInformation("Station operation: {Operation} | {Details}", 
                operation, details ?? "");
    }

    /// <summary>
    /// Logs an informational message for reading operations
    /// </summary>
    public static void LogReadingOperation(ILogger logger, string operation, Guid? readingId = null, Guid? stationId = null, string? details = null)
    {
        if (readingId.HasValue && stationId.HasValue)
            logger.LogInformation("Reading operation: {Operation} | ReadingId: {ReadingId} | StationId: {StationId} | {Details}", 
                operation, readingId.Value, stationId.Value, details ?? "");
        else if (stationId.HasValue)
            logger.LogInformation("Reading operation: {Operation} | StationId: {StationId} | {Details}", 
                operation, stationId.Value, details ?? "");
        else
            logger.LogInformation("Reading operation: {Operation} | {Details}", 
                operation, details ?? "");
    }

    /// <summary>
    /// Logs an informational message for AQI calculation
    /// </summary>
    public static void LogAqiCalculation(ILogger logger, Guid readingId, int aqiValue, string category, int? pm25Aqi = null, int? pm10Aqi = null)
    {
        logger.LogInformation("AQI calculated | ReadingId: {ReadingId} | AQI: {AqiValue} ({Category}) | PM2.5: {Pm25Aqi} | PM10: {Pm10Aqi}",
            readingId, aqiValue, category, pm25Aqi, pm10Aqi);
    }

    /// <summary>
    /// Logs an error with exception details
    /// </summary>
    public static void LogError(ILogger logger, string operation, Exception exception, string? details = null)
    {
        logger.LogError(exception, "Error during {Operation} | {Details}", operation, details ?? "");
    }

    /// <summary>
    /// Logs a warning message
    /// </summary>
    public static void LogWarning(ILogger logger, string operation, string message)
    {
        logger.LogWarning("{Operation} | {Message}", operation, message);
    }

    /// <summary>
    /// Logs application startup information
    /// </summary>
    public static void LogApplicationStartup(ILogger logger, string environment)
    {
        logger.LogInformation("AirAware application starting | Environment: {Environment}", environment);
    }

    /// <summary>
    /// Logs application configuration information
    /// </summary>
    public static void LogConfiguration(ILogger logger, string configKey, string configValue)
    {
        logger.LogInformation("Configuration | {ConfigKey}: {ConfigValue}", configKey, configValue);
    }
}
