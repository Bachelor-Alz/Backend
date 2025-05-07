using HealthDevice.Models;

namespace HealthDevice.DTO;



public class PostHeartRate
{
   public Heartrate? Heartrate { get; set; }
}

public class PostSpo2
{
   public Spo2? Spo2 { get; set; }
}

public class currentData
{
    public DateTime Timestamp { get; set; }
}

public class currentHeartRate : currentData
{
    public int Heartrate { get; set; }
}

public class currentSpo2 : currentData
{
    public float SpO2 { get; set; }
}