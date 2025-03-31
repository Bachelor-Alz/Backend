namespace HealthDevice.DTO;

public class Heartrate
{
    public int Id { get; set; }
    public int Maxrate { get; set; }
    public int Minrate { get; set; }
    public int Avgrate { get; set; }
    public DateTime Timestamp { get; set; }
}

//We need to send to frontend every hour send AvgRate. If min or max is out of range send asap

public class Spo2
{
    public int Id { get; set; }
    public float SpO2 { get; set; }
    public float MaxSpO2 { get; set; }
    public float MinSpO2 { get; set; }
    public DateTime Timestamp { get; set; }
}

//We need to send to frontend every hour Min and Max and the last value Spo2

public class Steps
{
    public int Id { get; set; }
    public int StepsCount { get; set; }
    public DateTime Timestamp { get; set; }
}

//We need to send to frontend every hour send step count