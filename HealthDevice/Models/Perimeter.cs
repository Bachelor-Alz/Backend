namespace HealthDevice.Models;

public class Perimeter
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int Radius { get; set; }
    public int Id { get; set; }
    public string? MacAddress { get; set; }
}
