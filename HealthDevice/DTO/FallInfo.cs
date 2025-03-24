namespace HealthDevice.DTO;

public class FallInfo
{
    public required int Id { get; set; }
    public DateTime timestamp { get; set; }
    public Location location { get; set; }
    public string status { get; set; }
}