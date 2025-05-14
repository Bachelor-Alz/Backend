using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HealthDevice.Models;

namespace HealthDevice.DTO;

public abstract class Sensor
{
    [Key]
    public int Id { get; set; }
    public string? MacAddress { get; set; }
    [ForeignKey("MacAddress")]
    public Arduino? Arduino { get; set; }
    public DateTime Timestamp { get; set; }
}