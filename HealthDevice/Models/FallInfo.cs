namespace HealthDevice.Models;

public class FallInfo
{
    public required int id { get; set; }
    public DateTime timestamp { get; set; }
    public Location location { get; set; }
    public string status { get; set; }
}