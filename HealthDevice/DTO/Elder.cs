using System.ComponentModel.DataAnnotations.Schema;
using HealthDevice.Models;
using Microsoft.AspNetCore.Identity;

namespace HealthDevice.DTO;

public class ElderLocation
{
    public string name { get; set; }
    public string email { get; set; }
    public double? latitude { get; set; }
    public double? longitude { get; set; }
    public Perimeter? perimeter { get; set; }
    public DateTime lastUpdated { get; set; }
}

public class GetElderDTO
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public Roles role { get; set; }
}