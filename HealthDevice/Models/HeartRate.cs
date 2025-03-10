namespace HealthDevice.Models;

public class Heartrate
{
    public required int Id { get; set; }
    public int Rate { get; set; }
    public DateTime Timestamp { get; set; }
    public required Elder elder { get; set; }
}