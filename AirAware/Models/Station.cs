namespace AirAware.Models;

public class Station
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required double Latitude { get; set; }
    public required double Longitude { get; set; }
    public string? Provider { get; set; }
    public string? Metadata { get; set; }
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}