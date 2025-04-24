using HealthDevice.DTO;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
namespace HealthDevice.Services;


public class UserService
{
    private readonly ILogger<UserService> _logger;
    private readonly UserManager<Elder> _elderManager;
    private readonly UserManager<Caregiver> _caregiverManager;
    
    public UserService(ILogger<UserService> logger, UserManager<Elder> elderManager, UserManager<Caregiver> caregiverManager)
    {
        _logger = logger;
        _elderManager = elderManager;
        _caregiverManager = caregiverManager;
    }
    
    public async Task<ActionResult<LoginResponseDTO>> HandleLogin(UserLoginDTO userLoginDto, HttpContext httpContext)
    {
        string ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
        DateTime timestamp = DateTime.UtcNow;
        Elder? elder = await _elderManager.FindByEmailAsync(userLoginDto.Email);
        if (elder != null)
        {
            if(!await _elderManager.CheckPasswordAsync(elder, userLoginDto.Password))
            {
                _logger.LogWarning("Login failed for email: {Email} from IP: {IpAddress} at {Timestamp}.", userLoginDto.Email, ipAddress, timestamp);
                return new UnauthorizedResult();
            }
            if (await _elderManager.IsLockedOutAsync(elder))
            {
                _logger.LogWarning("Account locked out: {Email} from IP: {IpAddress} at {Timestamp}.", userLoginDto.Email, ipAddress, timestamp);
                return new UnauthorizedResult();
            }
            _logger.LogInformation("Login successful for email: {Email} from IP: {IpAddress} at {Timestamp}.", userLoginDto.Email, ipAddress, timestamp);
            return new LoginResponseDTO { Token = GenerateJwt(elder, "Elder"), role = Roles.Elder };
        }

        Caregiver? caregiver = await _caregiverManager.FindByEmailAsync(userLoginDto.Email);
        if (caregiver == null)
        {
            _logger.LogInformation("Couldnt find a user with the email {Email} from IP: {IpAddress} at {Timestamp}.", userLoginDto.Email, ipAddress, timestamp);
            return new UnauthorizedResult();
        }
        if (!await _caregiverManager.CheckPasswordAsync(caregiver, userLoginDto.Password))
        {
            _logger.LogWarning("Login failed for email: {Email} from IP: {IpAddress} at {Timestamp}.", userLoginDto.Email, ipAddress, timestamp);
            return new UnauthorizedResult();
        }
        if (await _caregiverManager.IsLockedOutAsync(caregiver))
        {
            _logger.LogWarning("Account locked out: {Email} from IP: {IpAddress} at {Timestamp}.", userLoginDto.Email, ipAddress, timestamp);
            return new UnauthorizedResult();
        }
        _logger.LogInformation("Login successful for email: {Email} from IP: {IpAddress} at {Timestamp}.", userLoginDto.Email, ipAddress, timestamp);
        return new LoginResponseDTO { Token = GenerateJwt(caregiver, "Caregiver"), role = Roles.Caregiver};
    }

    public async Task<ActionResult> HandleRegister<T>(UserManager<T> userManager, UserRegisterDTO userRegisterDto, T user, HttpContext httpContext) where T : IdentityUser
    {
        string ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
        DateTime timestamp = DateTime.UtcNow;

        if (await userManager.FindByEmailAsync(userRegisterDto.Email) != null)
        {
            _logger.LogWarning("{timestamp}: Registration failed for email: {Email} from IP: {IpAddress} - Email exists.", userRegisterDto.Email, ipAddress, timestamp);
            return new BadRequestObjectResult("Email already exists.");
        }

        IdentityResult result = await userManager.CreateAsync(user, userRegisterDto.Password);

        if (!result.Succeeded)
            return new BadRequestObjectResult(new { Message = "Registration failed.", result.Errors });
        _logger.LogInformation("{timestamp}: Registration successful for email: {Email} from IP: {IpAddress}.", userRegisterDto.Email, ipAddress, timestamp);
        return new OkResult();

    }

    private static string GenerateJwt<T>(T user, string role) where T : IdentityUser
    {
        SymmetricSecurityKey securityKey = new SymmetricSecurityKey("Your_32_Character_Long_Secret_Key_Here"u8.ToArray());
        SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        if (user.Email == null) return string.Empty;
        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, role)
        ];

        JwtSecurityToken? token = new JwtSecurityToken(
            issuer: "api.healthdevice.com",
            audience: "user.healthdevice.com",
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}