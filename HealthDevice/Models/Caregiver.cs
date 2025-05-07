using Microsoft.AspNetCore.Identity;

namespace HealthDevice.Models;

public class Caregiver : IdentityUser
{
    public required string Name { get; set; }
    
    public List<Elder>? Elders { get; set; }
    
    public List<Elder>? Invites { get; set; }
}