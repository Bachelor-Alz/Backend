namespace HealthDevice.DTO;

public class ArduinoDTO
{
    public int Id { get; set; }
    public double Latitude { get; set; }         // Decimal degrees (positive = N, negative = S)
    public double Longitude { get; set; }        // Decimal degrees (positive = E, negative = W)
    public List<ArduinoMaxDTO> Max30102 { get; set; }
    public int steps { get; set; }
    public string MacAddress { get; set; } = string.Empty;
}

public class ArduinoMaxDTO
{
    public int heartRate { get; set; }
    public float SpO2 { get; set; }
}

public class ArduinoInfoDTO
{
    public int Id { get; set; }
    public string MacAddress { get; set; }
    public string Address { get; set; }
    public float Distance { get; set; }
    public int lastActivity { get; set; }
}