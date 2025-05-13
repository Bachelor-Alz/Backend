namespace HealthDevice.DTO
{
    public class UserLoginDTO
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class LoginResponseDTO
    {
        public string? Token { get; set; }
        public Roles Role { get; set; }
        public string id { get; set; } = string.Empty;
        public required string RefreshToken { get; set; }
    }

    public class UserRegisterDTO
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public required Roles Role { get; set; }
    }
}