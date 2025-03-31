namespace HealthDevice.DTO;

public abstract class Sensor
{
    public string Address { get; set; }
    public long EpochTimestamp { get; set; }
    public DateTime Timestamp { get; set; }
}