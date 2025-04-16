using System.ComponentModel.DataAnnotations.Schema;

namespace HealthDevice.DTO;

public abstract class Sensor
{
    public string? Address { get; set; }
    [NotMapped]
    public DateTime Timestamp { get; set; }
}