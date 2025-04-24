namespace HealthDevice.DTO;

public class FallInfo
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public Location? Location { get; set; }
    public string? MacAddress { get; set; }
}

public class FallDTO
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public int fallCount { get; set; }
}

//We need to send when a fall is detected