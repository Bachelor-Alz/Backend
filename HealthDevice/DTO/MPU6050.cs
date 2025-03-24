namespace HealthDevice.DTO;

public class MPU6050
{
    public int Id { get; set; }
    public float AccelerationX { get; set; }
    public float AccelerationY { get; set; }
    public float AccelerationZ { get; set; }
    public float GyroscopeX { get; set; }
    public float GyroscopeY { get; set; }
    public float GyroscopeZ { get; set; }
    public DateTime Timestamp { get; set; }
}