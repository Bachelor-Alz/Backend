using System.ComponentModel.DataAnnotations.Schema;

namespace HealthDevice.DTO;

public abstract class Sensor
{
    public string Address { get; set; }
    [NotMapped]
    public long EpochTimestamp { get; set; }
    public DateTime Timestamp { get; set; }
}