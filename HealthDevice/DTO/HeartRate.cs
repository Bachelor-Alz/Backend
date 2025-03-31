namespace HealthDevice.DTO;

public class Heartrate
{
    public required int Id { get; set; }
    public int MaxRate { get; set; }
    public int MinRate { get; set; }
    public int AvgRate { get; set; }
    public DateTime Timestamp { get; set; }
}

//We need to send to frontend every hour send AvgRate. If min or max is out of range send asap

public class Spo2
{
    public required int Id { get; set; }
    public float spO2 { get; set; }
    public float MaxSpO2 { get; set; }
    public float MinSpO2 { get; set; }
    public DateTime Timestamp { get; set; }
}

//We need to send to frontend every hour Min and Max and the last value Spo2

public class Steps
{
    public required int Id { get; set; }
    public int distance { get; set; }
    public DateTime Timestamp { get; set; }
}

//We need to send to frontend every hour send step count