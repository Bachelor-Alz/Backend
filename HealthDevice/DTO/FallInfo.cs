namespace HealthDevice.DTO;

public class FallInfo
{
    public int Id { get; set; }
    public DateTime timestamp { get; set; }
    public Location location { get; set; }
}

//We need to send when a fall is detected