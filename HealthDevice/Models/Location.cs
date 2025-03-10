﻿namespace HealthDevice.Models;

public class Location
{
    public required int id { get; set; }
    public required Elder elder { get; set; }
    public int latitude { get; set; }
    public int longitude { get; set; }
    public int altitude { get; set; }
    public DateTime timestamp { get; set; }
}