namespace HealthDevice.DTO;

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
    public required string Role { get; set; }
}