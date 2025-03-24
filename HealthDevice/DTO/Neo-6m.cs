namespace HealthDevice.DTO;

public class Neo_6m
{
    public int Id { get; set; }
    public TimeSpan UtcTime { get; set; }        // hh:mm:ss
    public double Latitude { get; set; }         // Decimal degrees
    public char LatitudeDirection { get; set; }  // 'N' or 'S'
    public double Longitude { get; set; }        // Decimal degrees
    public char LongitudeDirection { get; set; } // 'E' or 'W'
    public float Course { get; set; }            // Track angle in degrees (true north)
    public DateOnly Date { get; set; }           // ddMMyy format
}