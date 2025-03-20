using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace HealthDevice.DTO
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
    public class UserLoginDTO
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class LoginResponseDTO
    {
        public string Token { get; set; }
    }

    public class UserRegisterDTO
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required Roles Role { get; set; }
    }

    public class UserTempDTO
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public Roles Role { get; set; }
    }
}