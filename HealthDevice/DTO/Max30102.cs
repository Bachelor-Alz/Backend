using System.ComponentModel.DataAnnotations.Schema;

namespace HealthDevice.DTO;

public class Max30102 : Sensor
{
    public int Id { get; set; }                 
    [NotMapped]
    public int Heartrate { get; set; }     
    public float SpO2 { get; set; }     
}