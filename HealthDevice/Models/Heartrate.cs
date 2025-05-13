using HealthDevice.DTO;

namespace HealthDevice.Models;

public class Heartrate : Sensor
{
    public int Id { get; set; }
    public int Maxrate { get; set; }
    public int Minrate { get; set; }
    public int Avgrate { get; set; }
    public int Lastrate { get; set; }
}