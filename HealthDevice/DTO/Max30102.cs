using System.ComponentModel.DataAnnotations.Schema;

namespace HealthDevice.DTO;

public class Max30102
{
    public int Id { get; set; }                 
    [NotMapped]
    public long EpochTimestamp { get; set; } 
    public DateTime Timestamp { get; set; }
    public int HeartRate { get; set; }     
    public float SpO2 { get; set; }       
}