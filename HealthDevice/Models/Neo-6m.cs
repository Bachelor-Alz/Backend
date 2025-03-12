namespace HealthDevice.Models;

public class Neo_6m
{
    public int Id { get; set; }
    public TimeSpan UtcTime { get; set; }        // hh:mm:ss
    public char Status { get; set; }             // 'A' (valid) or 'V' (warning)
    public double Latitude { get; set; }         // Decimal degrees
    public char LatitudeDirection { get; set; }  // 'N' or 'S'
    public double Longitude { get; set; }        // Decimal degrees
    public char LongitudeDirection { get; set; } // 'E' or 'W'
    public float SpeedKnots { get; set; }        // Speed in knots
    public float Course { get; set; }            // Track angle in degrees (true north)
    public DateOnly Date { get; set; }           // ddMMyy format
    public float? MagneticVariation { get; set; } // Magnetic variation in degrees (nullable if missing)
    public char? MagneticDirection { get; set; } // 'E' or 'W' (nullable if missing)
    public byte Checksum { get; set; }           // XOR checksum value
}