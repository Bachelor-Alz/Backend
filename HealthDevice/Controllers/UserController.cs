using HealthDevice.DTO;
using HealthDevice.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthDevice.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly UserManager<Elder> _elderManager;
    private readonly UserManager<Caregiver> _caregiverManager;
    private readonly UserService _userService;
    
    public UserController(UserManager<Elder> elderManager, UserManager<Caregiver> caregiverManager,
        UserService userService)
    {
        _elderManager = elderManager;
        _caregiverManager = caregiverManager;
        _userService = userService;
    }
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDTO>> Login(UserLoginDTO userLoginDTO)
    {
        return userLoginDTO.Role == Roles.Elder 
            ? await _userService.HandleLogin(_elderManager, userLoginDTO, "Elder", HttpContext) 
            : await _userService.HandleLogin(_caregiverManager, userLoginDTO, "Caregiver", HttpContext);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult> Register(UserRegisterDTO userRegisterDTO)
    {
        return userRegisterDTO.Role == Roles.Elder 
            ? await _userService.HandleRegister(_elderManager, userRegisterDTO, new Elder { name = userRegisterDTO.Name, Email = userRegisterDTO.Email, UserName = userRegisterDTO.Email, location = new Location { id = 0 }, heartrates = new List<Heartrate>() }, HttpContext)
            : await _userService.HandleRegister(_caregiverManager, userRegisterDTO, new Caregiver { name = userRegisterDTO.Name, Email = userRegisterDTO.Email, UserName = userRegisterDTO.Email, elders = new List<Elder>() }, HttpContext);
    }

    [HttpGet("users")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult<List<Elder>>> GetUsers() => await _elderManager.Users.ToListAsync();

    [HttpGet("users/{email}")]
    [Authorize(Roles = "Elder")]
    public async Task<ActionResult<Elder>> GetUser(string email)
    {
        Elder? user = await _elderManager.FindByEmailAsync(email);
        return user == null ? NotFound() : user;
    }
}
