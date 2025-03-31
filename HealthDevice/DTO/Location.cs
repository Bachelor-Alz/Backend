namespace HealthDevice.DTO;

public class Location
{
    public int Id { get; set; }
    public int Latitude { get; set; }
    public int Longitude { get; set; }
    public DateTime Timestamp { get; set; }
}

public class Perimeter
{
    public Location Location { get; set; }
    public int Radius { get; set; }

    public int Id { get; set; }
}

public class Kilometer
{
    public int Id { get; set; }
    public double Distance { get; set; }
    public DateTime Timestamp { get; set; }
}
