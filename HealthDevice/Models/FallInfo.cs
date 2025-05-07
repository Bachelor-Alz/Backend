namespace HealthDevice.Models;

public class FallInfo
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public Location? Location { get; set; }
    public string? MacAddress { get; set; }
}