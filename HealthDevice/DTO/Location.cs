namespace HealthDevice.DTO;

public class Location
{
    public int id { get; set; }
    public int latitude { get; set; }
    public int longitude { get; set; }
    public DateTime timestamp { get; set; }
}

public class Perimeter
{
    public Location location { get; set; }
    public int radius { get; set; }

    public int Id { get; set; }
}