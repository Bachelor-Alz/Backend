using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HealthDevice.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HealthDevice.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly UserManager<Elder> _elderManager;
    private readonly UserManager<Caregiver> _caregiverManager;
    private readonly ILogger<UserController> _logger;

    public UserController(UserManager<Elder> elderManager, UserManager<Caregiver> caregiverManager,
        ILogger<UserController> logger)
    {
        _elderManager = elderManager;
        _caregiverManager = caregiverManager;
        _logger = logger;
    }
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDTO>> Login(UserLoginDTO userLoginDTO)
    {

        string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
        DateTime timestamp = DateTime.UtcNow;
        if (userLoginDTO.Role == Roles.Elder)
        {
            Elder? user = await _elderManager.FindByEmailAsync(userLoginDTO.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed for email: {Email} from IP: {IpAddress} at {Timestamp} - User not found.", 
                    userLoginDTO.Email, 
                    ipAddress, 
                    timestamp);
                return Unauthorized("User not found.");
            }

            if (!await _elderManager.CheckPasswordAsync(user, userLoginDTO.Password))
            {
                _logger.LogWarning("Login failed for email: {Email} from IP: {IpAddress} at {Timestamp} - Incorrect password.", 
                    userLoginDTO.Email, 
                    ipAddress, 
                    timestamp);
                return Unauthorized("Incorrect password.");
            }
        
            if (await _elderManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Login failed for email: {Email} from IP: {IpAddress} at {Timestamp} - Account is locked out.", 
                    userLoginDTO.Email, 
                    ipAddress, 
                    timestamp);
                return Unauthorized("Account is locked out.");
            }

            // Log successful login
            _logger.LogInformation("Login successful for email: {Email} from IP: {IpAddress} at {Timestamp}. Generating JWT.", 
                userLoginDTO.Email, 
                ipAddress, 
                timestamp);

            // Generate JWT
            string token = GenerateJWT(user);

            return new LoginResponseDTO
            {
                Token = token
            };
        }
        if (userLoginDTO.Role == Roles.Caregiver)
        {
            Caregiver? user = await _caregiverManager.FindByEmailAsync(userLoginDTO.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed for email: {Email} from IP: {IpAddress} at {Timestamp} - User not found.", 
                    userLoginDTO.Email, 
                    ipAddress, 
                    timestamp);
                return Unauthorized("User not found.");
            }

            if (!await _caregiverManager.CheckPasswordAsync(user, userLoginDTO.Password))
            {
                _logger.LogWarning("Login failed for email: {Email} from IP: {IpAddress} at {Timestamp} - Incorrect password.", 
                    userLoginDTO.Email, 
                    ipAddress, 
                    timestamp);
                return Unauthorized("Incorrect password.");
            }
        
            if (await _caregiverManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Login failed for email: {Email} from IP: {IpAddress} at {Timestamp} - Account is locked out.", 
                    userLoginDTO.Email, 
                    ipAddress, 
                    timestamp);
                return Unauthorized("Account is locked out.");
            }

            // Log successful login
            _logger.LogInformation("Login successful for email: {Email} from IP: {IpAddress} at {Timestamp}. Generating JWT.", 
                userLoginDTO.Email, 
                ipAddress, 
                timestamp);

            // Generate JWT
            string token = GenerateJWT(user);

            return new LoginResponseDTO
            {
                Token = token
            };
        }

        return BadRequest();
    }


    
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult> Register(UserRegisterDTO userRegisterDTO)
    {
        string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
        DateTime timestamp = DateTime.UtcNow;
        if (userRegisterDTO.Role == Roles.Elder)
        {
            string email = userRegisterDTO.Email;
        
            if(await _elderManager.FindByEmailAsync(email) != null)
            {
                _logger.LogWarning("Registration failed for email: {Email} from IP: {IpAddress} at {Timestamp} - Email already exists.", 
                    email, 
                    ipAddress, 
                    timestamp);
                return BadRequest("Email already exists.");
            }
        
            Elder user = new Elder {
            
                name = userRegisterDTO.Name,
                Email = userRegisterDTO.Email,
                UserName = userRegisterDTO.Email,
                locations = new Location
                {
                    id = 0
                },
                heartrates = new List<Heartrate>()
            };
        
            var result = await _elderManager.CreateAsync(user, userRegisterDTO.Password);
        
        
            if(result.Succeeded)
            {
                _logger.LogInformation("Registration successful for email: {Email} from IP: {IpAddress} at {Timestamp}. Generating JWT.", 
                    email, 
                    ipAddress, 
                    timestamp);
                return Ok();
            }
            return BadRequest(new { Message = "Registration failed.", Errors = result.Errors });
        }
        if (userRegisterDTO.Role == Roles.Caregiver)
        {
            string email = userRegisterDTO.Email;
        
            if(await _caregiverManager.FindByEmailAsync(email) != null)
            {
                _logger.LogWarning("Registration failed for email: {Email} from IP: {IpAddress} at {Timestamp} - Email already exists.", 
                    email, 
                    ipAddress, 
                    timestamp);
                return BadRequest("Email already exists.");
            }
        
            Caregiver user = new Caregiver {
            
                name = userRegisterDTO.Name,
                Email = userRegisterDTO.Email,
                UserName = userRegisterDTO.Email,
                elders = new List<Elder>()
            };
        
            var result = await _caregiverManager.CreateAsync(user, userRegisterDTO.Password);
        
        
            if(result.Succeeded)
            {
                _logger.LogInformation("Registration successful for email: {Email} from IP: {IpAddress} at {Timestamp}. Generating JWT.", 
                    email, 
                    ipAddress, 
                    timestamp);
                return Ok();
            }
            return BadRequest(new { Message = "Registration failed.", Errors = result.Errors });
        }

        return BadRequest();
    }
    
    
    [HttpGet("users")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult<List<Elder>>> GetUsers()
    {
        List<Elder> users = await _elderManager.Users.ToListAsync();
        return users;

    }
    
    
    [HttpGet("users/{email}")]
    [Authorize(Roles = "Elder")]
    public async Task<ActionResult<Elder>> GetUser(string email)
    {
        Elder? user = await _elderManager.FindByEmailAsync(email);
        if (user == null)
        {
            return NotFound();
        }
        return user;
    }
    
    //Need to implement JWT
    private string GenerateJWT(Elder user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Your_32_Character_Long_Secret_Key_Here"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "Elder")
        };

        var token = new JwtSecurityToken(
            issuer: "api.healthdevice.com",
            audience: "user.healthdevice.com",
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateJWT(Caregiver user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Your_32_Character_Long_Secret_Key_Here"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "Caregiver")
        };

        var token = new JwtSecurityToken(
            issuer: "api.healthdevice.com",
            audience: "user.healthdevice.com",
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}