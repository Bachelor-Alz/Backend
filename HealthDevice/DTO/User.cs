using Microsoft.AspNetCore.Identity;

namespace HealthDevice.DTO
{
    public class User : IdentityUser
    {
        public required string name { get; set; }
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
}