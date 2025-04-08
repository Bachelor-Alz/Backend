using Microsoft.AspNetCore.Identity;

namespace HealthDevice.DTO;

public class Caregiver : IdentityUser
{
    public required string Name { get; set; }
    public List<Elder>? Elders { get; set; }
}