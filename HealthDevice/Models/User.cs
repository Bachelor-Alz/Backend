using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace HealthDevice.Models
{
    public class User : IdentityUser
    {
        public required string name { get; set; }

        [Key]
        [JsonPropertyName("userEmail")]
        public required string email { get; set; }

        public required string password { get; set; }
        public required Roles Role { get; set; }
    }
}