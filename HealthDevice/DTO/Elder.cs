using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace HealthDevice.DTO;

public class Elder : IdentityUser
{
    public required string Name { get; set; }
    public DashBoard? dashBoard { get; set; }
    public string? MacAddress { get; set; }
    public double latitude { get; set; }
    public double longitude { get; set; }
    public bool outOfPerimeter { get; set; }
    
    // Foreign key for the assigned caregiver
    public string? CaregiverId { get; set; }
    [ForeignKey("CaregiverId")]
    public Caregiver? Caregiver { get; set; }

    // Foreign key for the invited caregiver
    public string? InvitedCaregiverId { get; set; }
    [ForeignKey("InvitedCaregiverId")]
    public Caregiver? InvitedCaregiver { get; set; }
}

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