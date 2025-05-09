namespace HealthDevice.DTO;

public abstract class Sensor
{
    public required string MacAddress { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}