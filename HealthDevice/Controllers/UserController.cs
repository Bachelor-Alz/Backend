using HealthDevice.Data;
using HealthDevice.DTO;
using HealthDevice.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthDevice.Controllers;

[Authorize]
[ApiController]
public class UserController : ControllerBase
{
    
    private readonly UserContext _context;
    
    public UserController(UserContext context)
    {
        _context = context;
    }
    
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDTO>> Login(UserLoginDTO userLoginDTO)
    {
        User? user = await _context.Users.FindAsync(userLoginDTO.Email);
        
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
    [HttpPost("register")]
    public async Task<ActionResult> Register(UserRegisterDTO userRegisterDTO)
    {
        User user = new User
        {
            email = userRegisterDTO.Email,
            name = userRegisterDTO.Name,
            role = userRegisterDTO.Role
        };
        //Need some hashing here
        user.password = userRegisterDTO.Password;
        
        _context.Users.Add(user);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            return BadRequest();
        }
        
        return Ok();
    }
    
    [HttpPost("users")]
    public async Task<ActionResult<List<User>>> GetUsers()
    {
        List<User> users = await _context.Users.ToListAsync();
        return users;

    }
    
    public async Task<ActionResult<User>> GetUser(string email)
    {
        User? user = await _context.Users.FindAsync(email);
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