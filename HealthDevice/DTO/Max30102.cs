namespace HealthDevice.DTO;

public class Max30102 : Sensor
{
    public int Id { get; set; }                 
    public Heartrate Heartrate { get; set; }     
    public Spo2 SpO2 { get; set; }     
}