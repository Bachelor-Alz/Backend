using System.ComponentModel.DataAnnotations;

namespace HealthDevice.Models;

public class Perimeter
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? Radius { get; set; }
    public int Id { get; set; }
    [MaxLength(18)]
    public string? MacAddress { get; set; }
}
