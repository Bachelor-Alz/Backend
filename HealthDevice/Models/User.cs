using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace HealthDevice.Models;

public class User : IdentityUser
{
    public string name { get; set; }
    [Key]
    public string email { get; set; }
    public string password { get; set; }
    public string role { get; set; }
}