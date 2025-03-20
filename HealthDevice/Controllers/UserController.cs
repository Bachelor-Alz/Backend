using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HealthDevice.Data;
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
    
    public UserController(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDTO>> Login(UserLoginDTO userLoginDTO)
    {
        User user = await _userManager.FindByEmailAsync(userLoginDTO.Email);
        if (user == null)
        {
            return Unauthorized();
        }
        if (!await _userManager.CheckPasswordAsync(user, userLoginDTO.Password))
        {
            return Unauthorized();
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
    public async Task<ActionResult> Register(UserRegisterDTO userRegisterDTO)
    {
        if (userRegisterDTO == null)
        {
            return BadRequest();
        }
        
        User user = new User {
            name = userRegisterDTO.Name,
            email = userRegisterDTO.Email,
            password = userRegisterDTO.Password,
            Role = userRegisterDTO.Role
        };
        
        var result = await _userManager.CreateAsync(user, userRegisterDTO.Password);
        
        if(result.Succeeded)
        {
            return Ok();
        }
        return BadRequest();
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