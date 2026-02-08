using System.Text.Json.Serialization;

namespace AirAware.Models;

public class AqiRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReadingId { get; set; }
    [JsonIgnore]
    public Reading Reading { get; set; } = null!;
    public Guid StationId { get; set; }
    [JsonIgnore]
    public Station Station { get; set; } = null!;
    public int AqiValue { get; set; }
    public int? Pm25Aqi { get; set; }
    public int? Pm10Aqi { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Pm25Category { get; set; }
    public string? Pm10Category { get; set; }
    public DateTime ComputedAt { get; set; } = DateTime.Now;
}