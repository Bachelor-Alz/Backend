using System.ComponentModel.DataAnnotations;

namespace HealthDevice.Models;

public class Location
{
    public int Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; }
    [MaxLength(18)]
    public string? MacAddress { get; set; }
}