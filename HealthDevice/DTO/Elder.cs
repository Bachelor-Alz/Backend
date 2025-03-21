namespace HealthDevice.DTO;

public class Elder
{
    public required int id { get; set; }
    public required string name { get; set; }
    public List<Heartrate> heartrates { get; set; }
    public Location locations { get; set; }
}

public class ElderLocationDTO
{
    public required int id { get; set; }
    public required string name { get; set; }
}