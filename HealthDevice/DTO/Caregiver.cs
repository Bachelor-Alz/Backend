using Microsoft.AspNetCore.Identity;

namespace HealthDevice.DTO;

public class Caregiver : IdentityUser
{
    public required string Name { get; set; }
    
    public List<Elder>? Elders { get; set; }
    
    public List<Elder>? Invites { get; set; }
}

public class CaregiverDTO
{
    public string? Name { get; set; }
    public string? Email { get; set; }
}