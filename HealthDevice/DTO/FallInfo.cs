namespace HealthDevice.DTO;
public class FallDTO
{
    public int Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public int fallCount { get; set; }
}

public class StepsDTO
{
    public int Id { get; set; }
    public int StepsCount { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string? MacAddress { get; set; }
}

public class DistanceInfoDTO
{
    public int Id { get; set; }
    public float Distance { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string? MacAddress { get; set; }
}
