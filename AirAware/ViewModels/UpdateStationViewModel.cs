namespace AirAware.ViewModels;

public class UpdateStationViewModel
{
    public string? Name { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Provider { get; set; }
    public string? Metadata { get; set; }
    public bool? Active { get; set; }
}