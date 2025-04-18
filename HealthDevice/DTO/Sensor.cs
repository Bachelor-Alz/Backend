namespace HealthDevice.DTO;

public abstract class Sensor
{
    public string? Address { get; set; }
    public DateTime Timestamp { get; set; }
}