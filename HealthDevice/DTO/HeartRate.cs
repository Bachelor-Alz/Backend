using HealthDevice.Models;

namespace HealthDevice.DTO;

public class PostHeartRate
{
    public int Id { get; set; }
    public int Maxrate { get; set; }
    public int Minrate { get; set; }
    public int Avgrate { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string? MacAddress { get; set; }
}

public class PostSpO2
{
    public int Id { get; set; }
    public float AvgSpO2 { get; set; }
    public float MaxSpO2 { get; set; }
    public float MinSpO2 { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string? MacAddress { get; set; }
}