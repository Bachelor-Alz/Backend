using HealthDevice.DTO;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
namespace HealthDevice.Services;


public class UserService
{
    private readonly ILogger<UserService> _logger;

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
    }

    public async Task<ActionResult<LoginResponseDTO>> HandleLogin<T>(UserManager<T> userManager, UserLoginDTO userLoginDTO, string role, HttpContext httpContext) where T : IdentityUser
    {
        string ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
        DateTime timestamp = DateTime.UtcNow;
        T? user = await userManager.FindByEmailAsync(userLoginDTO.Email);
        
        if (user == null || !await userManager.CheckPasswordAsync(user, userLoginDTO.Password))
        {
            _logger.LogWarning("Login failed for email: {Email} from IP: {IpAddress} at {Timestamp}.", userLoginDTO.Email, ipAddress, timestamp);
            return new UnauthorizedResult();
        }
        
        if (await userManager.IsLockedOutAsync(user))
        {
            _logger.LogWarning("Account locked out: {Email} from IP: {IpAddress} at {Timestamp}.", userLoginDTO.Email, ipAddress, timestamp);
            return new UnauthorizedResult();
        }
        
        _logger.LogInformation("Login successful for email: {Email} from IP: {IpAddress} at {Timestamp}.", userLoginDTO.Email, ipAddress, timestamp);
        return new LoginResponseDTO { Token = GenerateJWT(user, role) };
    }

    public async Task<ActionResult> HandleRegister<T>(UserManager<T> userManager, UserRegisterDTO userRegisterDTO, T user, HttpContext httpContext) where T : IdentityUser
    {
        string ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
        DateTime timestamp = DateTime.UtcNow;

        if (await userManager.FindByEmailAsync(userRegisterDTO.Email) != null)
        {
            _logger.LogWarning("{timestamp}: Registration failed for email: {Email} from IP: {IpAddress} - Email exists.", userRegisterDTO.Email, ipAddress, timestamp);
            return new BadRequestObjectResult("Email already exists.");
        }

        IdentityResult result = await userManager.CreateAsync(user, userRegisterDTO.Password);
        if (result.Succeeded)
        {
            _logger.LogInformation("{timestamp}: Registration successful for email: {Email} from IP: {IpAddress}.", userRegisterDTO.Email, ipAddress, timestamp);
            return new OkResult();
        }
        return new BadRequestObjectResult(new { Message = "Registration failed.", Errors = result.Errors });
    }

    private string GenerateJWT<T>(T user, string role) where T : IdentityUser
    {
        SymmetricSecurityKey? securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Your_32_Character_Long_Secret_Key_Here"));
        SigningCredentials? credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        Claim[] claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        JwtSecurityToken? token = new JwtSecurityToken(
            issuer: "api.healthdevice.com",
            audience: "user.healthdevice.com",
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}