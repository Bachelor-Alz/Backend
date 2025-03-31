using Microsoft.AspNetCore.Identity;

namespace HealthDevice.DTO;

public class Elder : IdentityUser
{
    public required string name { get; set; }
    public List<Max30102> Max30102Data { get; set; }
    public List<Heartrate> heartRate { get; set; }
    public List<GPS> GpsData { get; set; }
    public List<Spo2> spo2s { get; set; }
    public Location location { get; set; }
    public Perimeter perimeter { get; set; }
    public List<Kilometer> distance { get; set; }
    public string? arduino { get; set; }
    public FallInfo? fallInfo { get; set; }
    public List<Steps> steps { get; set; }
}

public class ElderLocationDTO
{
    public required int id { get; set; }
    public required string name { get; set; }
}