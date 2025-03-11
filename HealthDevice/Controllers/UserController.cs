using HealthDevice.DTO;
using HealthDevice.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthDevice.Controllers;

public class UserController : ControllerBase
{
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDTO>> Login(UserLoginDTO userLoginDTO)
    {
        return null;
    }
    
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult> Register(UserRegisterDTO userRegisterDTO)
    {
        return null;
    }
    
    [HttpPost("users")]
    public async Task<ActionResult> GetUsers()
    {
        return null;
    }
    
    public async Task<ActionResult> GetUser(string email)
    {
        return null;
    }
    
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        return null;
    }
    
    private string GenerateJWT(User user)
    {
        return null;
    }
    
}