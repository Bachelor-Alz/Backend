namespace HealthDevice.DTO;

public class Max30102
{
    public int Id { get; set; }                 // Unique ID
    public int HeartRate { get; set; }       // BPM (nullable if not calculated)
    public float SpO2 { get; set; }            // Blood oxygen percentage (nullable if not calculated)
    public DateTime Timestamp { get; set; }     // Timestamp of reading
}