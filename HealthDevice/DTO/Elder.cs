using Microsoft.AspNetCore.Identity;

namespace HealthDevice.DTO;

public class Elder : IdentityUser
{
    public required string Name { get; set; }
    public DashBoard? dashBoard { get; set; }
    public string? Arduino { get; set; }
    public double? latitude { get; set; }
    public double? longitude { get; set; }
}

public class ElderLocation
{
    public string name { get; set; }
    public string email { get; set; }
    public double? latitude { get; set; }
    public double? longitude { get; set; }
    public Perimeter? perimeter { get; set; }
}

public class GetElderDTO
{
    public string? Name { get; set; }
    public string? Email { get; set; }
}