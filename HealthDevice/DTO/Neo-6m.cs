using System.ComponentModel.DataAnnotations.Schema;

namespace HealthDevice.DTO;

public class GPS
{
    public int Id { get; set; }
    [NotMapped]
    public long EpochTimestamp { get; set; }     // Epoch millis from Arduino (used to compute Timestamp)
    public DateTime Timestamp { get; set; }      // Date and time in UTC
    public double Latitude { get; set; }         // Decimal degrees (positive = N, negative = S)
    public double Longitude { get; set; }        // Decimal degrees (positive = E, negative = W)
    public float Course { get; set; }            // Track angle in degrees
    
}