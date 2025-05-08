using HealthDevice.DTO;

namespace HealthDevice.Models;

public class FallInfo : Sensor
{
    public int Id { get; set; }
    public Location? Location { get; set; }
}
