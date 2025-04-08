using Microsoft.AspNetCore.Identity;

namespace HealthDevice.DTO;

public class Elder : IdentityUser
{
    public required string Name { get; set; }
    public List<Max30102>? MAX30102Data { get; set; }
    public List<Heartrate>? Heartrate { get; set; }
    public List<GPS>? GPSData { get; set; }
    public List<Spo2>? SpO2 { get; set; }
    public Location? Location { get; set; }
    public Perimeter? Perimeter { get; set; }
    public List<Kilometer>? Distance { get; set; }
    public string? Arduino { get; set; }
    public List<FallInfo>? FallInfo { get; set; }
    public List<Steps>? Steps { get; set; }
}

public class ElderLocationDTO
{
    public required int Id { get; set; }
    public required string Name { get; set; }
}