namespace HealthDevice.DTO;

public class Location
{
    public int Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; }
    public string? MacAddress { get; set; }
}

public class Perimeter
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int Radius { get; set; }

    public int Id { get; set; }
    public string? MacAddress { get; set; }
}

public class Kilometer
{
    public int Id { get; set; }
    public float Distance { get; set; }
    public DateTime Timestamp { get; set; }
    public string? MacAddress { get; set; }
}
