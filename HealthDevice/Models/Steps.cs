namespace HealthDevice.Models;

public class Steps
{
    public int Id { get; set; }
    public int StepsCount { get; set; }
    public DateTime Timestamp { get; set; }
    public string? MacAddress { get; set; }
}