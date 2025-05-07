namespace HealthDevice.Models;

public class Heartrate
{
    public int Id { get; set; }
    public int Maxrate { get; set; }
    public int Minrate { get; set; }
    public int Avgrate { get; set; }
    public DateTime Timestamp { get; set; }
    public string? MacAddress { get; set; }
}