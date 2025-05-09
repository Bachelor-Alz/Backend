namespace HealthDevice.DTO;

public class ArduinoDTO
{
    public double Latitude { get; set; }         // Decimal degrees (positive = N, negative = S)
    public double Longitude { get; set; }        // Decimal degrees (positive = E, negative = W)
    public required List<ArduinoMaxDTO> Max30102 { get; set; }
    public int Steps { get; set; }
    public string MacAddress { get; set; } = string.Empty;
}

public class ArduinoMaxDTO
{
    public int HeartRate { get; set; }
    public float SpO2 { get; set; }
}

public class ArduinoInfoDTO
{
    public int Id { get; set; }
    public required string MacAddress { get; set; }
    public required string Address { get; set; }
    public float Distance { get; set; }
    public int LastActivity { get; set; }
}

public class AiRequest
{
    public List<int> Predictions { get; set; }
    public string MacAddress { get; set; }
}