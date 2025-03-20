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
    private readonly UserManager<User> _userManager;
    private readonly ILogger<UserController> _logger;
    
    public UserController(UserManager<User> userManager, ILogger<UserController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDTO>> Login(UserLoginDTO userLoginDTO)
    {

        string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
        DateTime timestamp = DateTime.UtcNow;
        User? user = await _userManager.FindByEmailAsync(userLoginDTO.Email);
        if (user == null)
        {
            _logger.LogWarning("Login failed for email: {Email} from IP: {IpAddress} at {Timestamp} - User not found.", 
                                userLoginDTO.Email, 
                                ipAddress, 
                                timestamp);
            return Unauthorized("User not found.");
        }

        if (!await _userManager.CheckPasswordAsync(user, userLoginDTO.Password))
        {
            _logger.LogWarning("Login failed for email: {Email} from IP: {IpAddress} at {Timestamp} - Incorrect password.", 
                                userLoginDTO.Email, 
                                ipAddress, 
                                timestamp);
            return Unauthorized("Incorrect password.");
        }
        
        if (await _userManager.IsLockedOutAsync(user))
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


    
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult> Register(UserRegisterDTO userRegisterDTO)
    {
        string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
        DateTime timestamp = DateTime.UtcNow;
        string email = userRegisterDTO.Email;
        
        if(await _userManager.FindByEmailAsync(email) != null)
        {
            _logger.LogWarning("Registration failed for email: {Email} from IP: {IpAddress} at {Timestamp} - Email already exists.", 
                                email, 
                                ipAddress, 
                                timestamp);
            return BadRequest("Email already exists.");
        }
        
        User user = new User {
            name = userRegisterDTO.Name,
            Email = userRegisterDTO.Email,
            Role = userRegisterDTO.Role,
            UserName = userRegisterDTO.Email
        };
        
        var result = await _userManager.CreateAsync(user, userRegisterDTO.Password);
        
        
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
    
    
    [HttpGet("users")]
    public async Task<ActionResult<List<User>>> GetUsers()
    {
        List<User> users = await _userManager.Users.ToListAsync();
        return users;

    }
    
    
    [HttpGet("users/{email}")]
    public async Task<ActionResult<User>> GetUser(string email)
    {
        User? user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return NotFound();
        }
        return user;
    }
    
    //Need to implement JWT
    private string GenerateJWT(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Your_32_Character_Long_Secret_Key_Here"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
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