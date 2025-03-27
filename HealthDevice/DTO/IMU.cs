using System.ComponentModel.DataAnnotations.Schema;

namespace HealthDevice.DTO;

public class IMU
{
    public int Id { get; set; }
    [NotMapped]
    public long EpochTimestamp { get; set; }
    public DateTime Timestamp { get; set; }
    public float AccelerationX { get; set; }
    public float AccelerationY { get; set; }
    public float AccelerationZ { get; set; }
    public float GyroscopeX { get; set; }
    public float GyroscopeY { get; set; }
    public float GyroscopeZ { get; set; }
}