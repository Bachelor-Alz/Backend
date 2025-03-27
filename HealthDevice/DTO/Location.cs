﻿namespace HealthDevice.DTO;

public class Location
{
    public required int id { get; set; }
    public int latitude { get; set; }
    public int longitude { get; set; }
    public DateTime timestamp { get; set; }
}

public class Perimiter
{
    public Location location { get; set; }
    public int radius { get; set; }

    public required int Id { get; set; }
}