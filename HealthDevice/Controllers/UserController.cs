using HealthDevice.Data;
using HealthDevice.DTO;
using HealthDevice.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthDevice.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDTO>> Login(UserLoginDTO userLoginDTO, ApplicationDbContext dBcontext)
    {
        User? user = await dBcontext.Users.FindAsync(userLoginDTO.Email);
        
        if (user == null)
        {
            return NotFound();
        }
        //Need some hashing here
        if (user.password != userLoginDTO.Password)
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
    [Route("register")]
    [HttpPost]
    public async Task<ActionResult> Register(UserRegisterDTO userRegisterDTO, ApplicationDbContext dBcontext)
    {
        Console.Out.WriteLine(userRegisterDTO);
        
        User user = new User
        {
            email = userRegisterDTO.Email,
            name = userRegisterDTO.Name,
            role = userRegisterDTO.Role
        };
        //Need some hashing here
        user.password = userRegisterDTO.Password;
        
        dBcontext.Users.Add(user);
        try
        {
            await dBcontext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return BadRequest("Email already exists");
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
        return null;
    }
    
}