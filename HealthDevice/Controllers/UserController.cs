using System.Security.Claims;
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
    private readonly ILogger<UserController> _logger;
    
    public UserController(UserManager<Elder> elderManager, UserManager<Caregiver> caregiverManager,
        UserService userService, ILogger<UserController> logger)
    {
        _elderManager = elderManager;
        _caregiverManager = caregiverManager;
        _userService = userService;
        _logger = logger;
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


    [HttpPost("users/elder")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult> PutElder(string ElderEmail)
    {
        Caregiver caregiver = await _caregiverManager.FindByEmailAsync(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        if(caregiver == null)
        {
            _logger.LogError("Caregiver not found.");
            return BadRequest();
        }
        Elder? elder = await _elderManager.FindByEmailAsync(ElderEmail);
        if(elder == null)
        {
            _logger.LogError("Elder not found.");
            return NotFound();
        }
        
        caregiver.elders.Add(elder);
        try
        {
            await _caregiverManager.UpdateAsync(caregiver);
            _logger.LogInformation("{elder.Email} add to Caregiver {caregiver.name}.", elder.Email, caregiver.name);
            return Ok();
        }
        catch
        {
            _logger.LogError("Failed to update caregiver.");
            return BadRequest();
        }
    }
    
    [HttpGet("users/{email}")]
    [Authorize(Roles = "Elder")]
    public async Task<ActionResult<Elder>> GetUser(string email)
    {
        Elder? user = await _elderManager.FindByEmailAsync(email);
        return user == null ? NotFound() : user;
    }
}
