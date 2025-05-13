namespace HealthDevice.DTO;
public class RefreshTokenResult
{
    public required string Token { get; set; }
}

public class RefreshAndAccessTokenResult
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}