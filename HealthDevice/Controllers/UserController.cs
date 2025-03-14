using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HealthDevice.Data;
using HealthDevice.DTO;
using HealthDevice.Models;
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
    private readonly PasswordHasher<User> _passwordHasher;

    public UserController(PasswordHasher<User> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDTO>> Login(UserLoginDTO userLoginDTO, ApplicationDbContext dBcontext)
    {
        User user = await dBcontext.Users.FindAsync(userLoginDTO.Email);
        
        if (user == null)
        {
            return NotFound();
        }
        
        if (_passwordHasher.VerifyHashedPassword(user, user.password, userLoginDTO.Password) == PasswordVerificationResult.Failed)
        {
            return BadRequest("Invalid password");
        }
        
        //Need to implement JWT
        string token = GenerateJWT(user);
        return new LoginResponseDTO
        {
            Token = token
        };
    }
    
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult> Register(UserRegisterDTO userRegisterDTO, ApplicationDbContext dBcontext)
    {
        User user = new User
        {
            email = userRegisterDTO.Email,
            name = userRegisterDTO.Name,
            password = "ASdwasdw",
            Role = userRegisterDTO.Role
        };
        string hashedPassword = _passwordHasher.HashPassword(user, userRegisterDTO.Password);
        user.password = hashedPassword;
        
        dBcontext.Users.Add(user);
        try
        {
            await dBcontext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return BadRequest();
        }
        
        return Ok();
    }
    
    
    [HttpGet("users")]
    public async Task<ActionResult<List<User>>> GetUsers(ApplicationDbContext dBcontext)
    {
        List<User> users = await dBcontext.Users.ToListAsync();
        return users;

    }
    
    
    [HttpGet("users/{email}")]
    public async Task<ActionResult<User>> GetUser(string email, ApplicationDbContext dBcontext)
    {
        User? user = await dBcontext.Users.FindAsync(email);
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
            new Claim(JwtRegisteredClaimNames.Sub, user.email),
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