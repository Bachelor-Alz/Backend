namespace HealthDevice.DTO;

public abstract class Sensor
{
    public string? MacAddress { get; set; }
    public DateTime Timestamp { get; set; }
}