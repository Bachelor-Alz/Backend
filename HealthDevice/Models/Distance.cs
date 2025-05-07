namespace HealthDevice.Models;

public class DistanceInfo
{
    public int Id { get; set; }
    public float Distance { get; set; }
    public DateTime Timestamp { get; set; }
    public string? MacAddress { get; set; }
}
