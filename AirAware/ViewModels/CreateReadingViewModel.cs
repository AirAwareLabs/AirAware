using System.ComponentModel.DataAnnotations;

namespace AirAware.ViewModels;

public class CreateReadingViewModel
{
    [Required]
    public Guid StationId { get; set; }
    [Required]
    public double Pm25 { get; set; }
    [Required]
    public double Pm10 { get; set; }
    public string? RawPayload { get; set; }
}