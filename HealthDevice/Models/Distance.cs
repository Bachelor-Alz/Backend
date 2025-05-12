using HealthDevice.DTO;

namespace HealthDevice.Models;

public class DistanceInfo : Sensor
{
    public int Id { get; set; }
    public float Distance { get; set; }
}
