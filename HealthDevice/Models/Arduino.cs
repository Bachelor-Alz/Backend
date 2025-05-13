using System.ComponentModel.DataAnnotations;
using HealthDevice.DTO;

namespace HealthDevice.Models;

public class Arduino
{
    [Key]
    public string MacAddress { get; set; }

    public bool isClaim { get; set; }
    public Elder? elder { get; set; }
    public List<Sensor>? Sensors { get; set; }
}