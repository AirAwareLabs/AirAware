using System.Text.Json.Serialization;

namespace AirAware.Models;

public class Reading
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StationId { get; set; }
    [JsonIgnore]
    public Station Station { get; set; } = null!;
    public double Pm25 { get; set; }
    public double? Pm10 { get; set; }
    public string? RawPayload { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}