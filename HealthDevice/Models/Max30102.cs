using HealthDevice.DTO;

namespace HealthDevice.Models;

public class Max30102 : Sensor
{
    public int Id { get; set; }
    public int LastHeartrate { get; set; }
    public int AvgHeartrate { get; set; }
    public int MaxHeartrate { get; set; }
    public int MinHeartrate { get; set; }
    public float LastSpO2 { get; set; }
    public float AvgSpO2 { get; set; }
    public float MaxSpO2 { get; set; }
    public float MinSpO2 { get; set; }
}