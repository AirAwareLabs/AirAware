using System.ComponentModel.DataAnnotations;

namespace AirAware.ViewModels;

public class CreateStationViewModel
{
    [Required]
    public string Name { get; set; }
    [Required]
    public double? Latitude { get; set; }
    [Required]
    public double? Longitude { get; set; }
    public string? Provider { get; set; }
    public string? Metadata { get; set; }
}