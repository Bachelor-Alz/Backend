using System.ComponentModel.DataAnnotations.Schema;

namespace HealthDevice.DTO;

public class GPS : Sensor
{
    public int Id { get; set; }
    [NotMapped]
    public double Latitude { get; set; }         // Decimal degrees (positive = N, negative = S)
    public double Longitude { get; set; }        // Decimal degrees (positive = E, negative = W)
    public float Course { get; set; }     // Track angle in degrees
    
}