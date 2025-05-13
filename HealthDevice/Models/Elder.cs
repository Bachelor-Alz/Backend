using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace HealthDevice.Models;

public class Elder : IdentityUser
{
    [MaxLength(30)]
    public required string Name { get; set; }
    [MaxLength(18)]
    public string? MacAddress { get; set; }
    [ForeignKey("MacAddress")]
    public Arduino? Arduino { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool OutOfPerimeter { get; set; }

    // Foreign key for the assigned caregiver
    public string? CaregiverId { get; set; }
    [ForeignKey("CaregiverId")]
    public Caregiver? Caregiver { get; set; }

    // Foreign key for the invited caregiver
    public string? InvitedCaregiverId { get; set; }
    [ForeignKey("InvitedCaregiverId")]
    public Caregiver? InvitedCaregiver { get; set; }
}