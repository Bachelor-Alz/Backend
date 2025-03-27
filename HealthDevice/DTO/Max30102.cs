using System.ComponentModel.DataAnnotations.Schema;

namespace HealthDevice.DTO;

public class Max30102
{
    public int Id { get; set; }                 
    [NotMapped]
    public long EpochTimestamp { get; set; }    // Epoch millis from Arduino (used to compute Timestamp)
    public DateTime Timestamp { get; set; }     // UTC DateTime
    public float? BPM { get; set; }       // BPM (nullable if not calculated)
    public float? SpO2 { get; set; }            // Blood oxygen percentage (nullable if not calculated)
}