using HealthDevice.Models;
namespace HealthDevice.DTO;

public class ElderLocationDTO
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public Perimeter? Perimeter { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class GetElderDTO
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public Roles Role { get; set; }
}