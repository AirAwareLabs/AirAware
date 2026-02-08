namespace AirAware.Models;

public class Station
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required double Latitude { get; set; }
    public required double Longitude { get; set; }
    public string? Provider { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}