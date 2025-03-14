namespace HealthDevice.DTO;

public class Heartrate
{
    public required int Id { get; set; }
    public int Rate { get; set; }
    public DateTime Timestamp { get; set; }
}