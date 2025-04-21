namespace HealthDevice.DTO;

public class Arduino
{
    public int Id { get; set; }
    public double Latitude { get; set; }         // Decimal degrees (positive = N, negative = S)
    public double Longitude { get; set; }        // Decimal degrees (positive = E, negative = W)
    public List<ArduinoMax> Max30102 { get; set; }
    public int steps { get; set; }
    public string MacAddress { get; set; } = string.Empty;
}

public class ArduinoMax
{
    public int heartRate { get; set; }
    public float SpO2 { get; set; }
}