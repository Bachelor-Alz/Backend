using HealthDevice.DTO;

namespace HealthDevice.Models;

public class GPSData : Sensor
{
    public int Id { get; set; }
    public double Latitude { get; set; }         // Decimal degrees (positive = N, negative = S)
    public double Longitude { get; set; }        // Decimal degrees (positive = E, negative = W)

}