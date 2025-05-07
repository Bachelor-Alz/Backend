using HealthDevice.DTO;

namespace HealthDevice.Models;

public class Spo2 : Sensor
{
    public int Id { get; set; }
    public float AvgSpO2 { get; set; }
    public float MaxSpO2 { get; set; }
    public float MinSpO2 { get; set; }
}