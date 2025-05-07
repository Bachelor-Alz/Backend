using HealthDevice.DTO;

namespace HealthDevice.Models;

public class Steps : Sensor
{
    public int Id { get; set; }
    public int StepsCount { get; set; }
}