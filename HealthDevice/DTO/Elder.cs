using Microsoft.AspNetCore.Identity;

namespace HealthDevice.DTO;

public class Elder : IdentityUser
{
    public required string name { get; set; }
    public List<Heartrate> heartrates { get; set; }
    public Location location { get; set; }
    public Perimeter perimeter { get; set; }
}

public class ElderLocationDTO
{
    public required int id { get; set; }
    public required string name { get; set; }
}