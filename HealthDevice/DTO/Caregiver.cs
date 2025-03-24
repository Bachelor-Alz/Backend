using Microsoft.AspNetCore.Identity;

namespace HealthDevice.DTO;

public class Caregiver : IdentityUser
{
    public required string name { get; set; }
    public List<Elder> elders { get; set; }
}