using HealthDevice.DTO;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HealthDevice.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
namespace HealthDevice.Services;


public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly UserManager<Elder> _elderManager;
    private readonly UserManager<Caregiver> _caregiverManager;
    private readonly IRepository<Elder> _elderRepository;
    private readonly IRepository<Caregiver> _caregiverRepository;

    public UserService(ILogger<UserService> logger, UserManager<Elder> elderManager, UserManager<Caregiver> caregiverManager, IRepository<Elder> elderRepository, IRepository<Caregiver> caregiverRepository)
    {
        _logger = logger;
        _elderManager = elderManager;
        _caregiverManager = caregiverManager;
        _elderRepository = elderRepository;
        _caregiverRepository = caregiverRepository;
    }

    public async Task<ActionResult<LoginResponseDTO>> HandleLogin(UserLoginDTO userLoginDto, string ipAdress)
    {
        DateTime timestamp = DateTime.UtcNow;
        Elder? elder = await _elderRepository.Query().FirstOrDefaultAsync(m => m.Email == userLoginDto.Email);
        if (elder != null)
        {
            if (!await _elderManager.CheckPasswordAsync(elder, userLoginDto.Password))
            {
                _logger.LogWarning("Login failed for email: {Email} from IP: {IpAddress} at {Timestamp}.", userLoginDto.Email, ipAdress, timestamp);
                return new UnauthorizedResult();
            }
            if (await _elderManager.IsLockedOutAsync(elder))
            {
                _logger.LogWarning("Account locked out: {Email} from IP: {IpAddress} at {Timestamp}.", userLoginDto.Email, ipAdress, timestamp);
                return new UnauthorizedResult();
            }
            _logger.LogInformation("Login successful for email: {Email} from IP: {IpAddress} at {Timestamp}.", userLoginDto.Email, ipAdress, timestamp);
            return new LoginResponseDTO { Token = GenerateJwt(elder, "Elder"), role = Roles.Elder };
        }

        Caregiver? caregiver = await _caregiverRepository.Query().FirstOrDefaultAsync(m => m.Email == userLoginDto.Email);
        if (caregiver == null)
        {
            _logger.LogInformation("Couldnt find a user with the email {Email} from IP: {IpAddress} at {Timestamp}.", userLoginDto.Email, ipAdress, timestamp);
            return new UnauthorizedResult();
        }
        if (!await _caregiverManager.CheckPasswordAsync(caregiver, userLoginDto.Password))
        {
            _logger.LogWarning("Login failed for email: {Email} from IP: {IpAddress} at {Timestamp}.", userLoginDto.Email, ipAdress, timestamp);
            return new UnauthorizedResult();
        }
        if (await _caregiverManager.IsLockedOutAsync(caregiver))
        {
            _logger.LogWarning("Account locked out: {Email} from IP: {IpAddress} at {Timestamp}.", userLoginDto.Email, ipAdress, timestamp);
            return new UnauthorizedResult();
        }
        _logger.LogInformation("Login successful for email: {Email} from IP: {IpAddress} at {Timestamp}.", userLoginDto.Email, ipAdress, timestamp);
        return new LoginResponseDTO { Token = GenerateJwt(caregiver, "Caregiver"), role = Roles.Caregiver };
    }

    public async Task<ActionResult> HandleRegister<T>(UserManager<T> userManager, UserRegisterDTO userRegisterDto, T user, string ipAddress) where T : IdentityUser
    {
        DateTime timestamp = DateTime.UtcNow;

        if (await userManager.Users.FirstOrDefaultAsync(m => m.Email == userRegisterDto.Email) != null)
        {
            _logger.LogWarning("{timestamp}: Registration failed for email: {Email} from IP: {IpAddress} - Email exists.", userRegisterDto.Email, ipAddress, timestamp);
            return new BadRequestObjectResult("Email already exists.");
        }
        IdentityResult result = await userManager.CreateAsync(user, userRegisterDto.Password);

        if (!result.Succeeded)
            return new BadRequestObjectResult(new { Message = "Registration failed.", result.Errors });
        _logger.LogInformation("{timestamp}: Registration successful for email: {Email} from IP: {IpAddress}.", userRegisterDto.Email, ipAddress, timestamp);
        return new OkObjectResult("Registration successful.");

    }

    public string GenerateJwt<T>(T user, string role) where T : IdentityUser
    {
        SymmetricSecurityKey securityKey = new SymmetricSecurityKey("UGVuaXNQZW5pc1BlbmlzUGVuaXNQZW5pc1BlbmlzUGVuaXNQZW5pc1BlbmlzUGVuaXNQZW5pc1Blbmlz"u8.ToArray());
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
            expires: DateTime.Now.AddMinutes(100),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}