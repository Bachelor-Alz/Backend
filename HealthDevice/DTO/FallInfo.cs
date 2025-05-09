namespace HealthDevice.DTO;
public class FallDTO
{
    public DateTimeOffset Timestamp { get; set; }
    public int FallCount { get; set; }
}

public class StepsDTO
{
    public int StepsCount { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

public class DistanceInfoDTO
{
    public float Distance { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}
