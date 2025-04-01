using System.Security.Claims;
using HealthDevice.Data;
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
    private readonly ApplicationDbContext _dbContext;
    
    public UserController(
        UserManager<Elder> elderManager,
        UserManager<Caregiver> caregiverManager,
        UserService userService,
        ILogger<UserController> logger,
        ApplicationDbContext dbContext)
    {
        _elderManager = elderManager;
        _caregiverManager = caregiverManager;
        _userService = userService;
        _logger = logger;
        _dbContext = dbContext;
    }
    
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDTO>> Login(UserLoginDTO userLoginDto)
    {
        return userLoginDto.Role == Roles.Elder 
            ? await _userService.HandleLogin(_elderManager, userLoginDto, "Elder", HttpContext) 
            : await _userService.HandleLogin(_caregiverManager, userLoginDto, "Caregiver", HttpContext);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult> Register(UserRegisterDTO userRegisterDto)
    {
        return userRegisterDto.Role == Roles.Elder 
            ? await _userService.HandleRegister(_elderManager, userRegisterDto, 
                                                new Elder
                                                {
                                                    Name = userRegisterDto.Name,
                                                    Email = userRegisterDto.Email, 
                                                    UserName = userRegisterDto.Email, 
                                                    MAX30102Data = new List<Max30102>(), 
                                                    GPSData = new List<GPS>(),
                                                    Location = new Location(),
                                                    Perimeter = new Perimeter{Location = new Location()},
                                                }, HttpContext)
            : await _userService.HandleRegister(_caregiverManager, userRegisterDto, 
                                                new Caregiver
                                                {
                                                    Name = userRegisterDto.Name, 
                                                    Email = userRegisterDto.Email, 
                                                    UserName = userRegisterDto.Email, 
                                                    Elders = new List<Elder>()
                                                }, HttpContext);
    }

    [HttpGet("elder")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult<List<Elder>>> GetUsers() => await _elderManager.Users.ToListAsync();


    [HttpPost("users/elder")]
    [Authorize(Roles = "Elder")]
    public async Task<ActionResult> PutCaregiver(string caregiverEmail)
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
        {
            _logger.LogError("User claim is null or empty.");
            return BadRequest("User claim is not available.");
        }

        Caregiver? caregiver = await _caregiverManager.FindByEmailAsync(caregiverEmail);
        if (caregiver == null)
        {
            _logger.LogError("Caregiver not found.");
            return BadRequest("Caregiver not found.");
        }

        Elder? elder = await _elderManager.FindByEmailAsync(userClaim.Value);
        if (elder == null)
        {
            _logger.LogError("Elder not found.");
            return NotFound("Elder not found.");
        }

        caregiver.Elders.Add(elder);
        try
        {
            await _caregiverManager.UpdateAsync(caregiver);
            _logger.LogInformation("{elder.Email} added to Caregiver {caregiver.name}.", elder.Email, caregiver.Name);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update caregiver.");
            return BadRequest("Failed to update caregiver.");
        }
    }

    [HttpDelete("users/elder")]
    [Authorize(Roles = "Elder")]
    public async Task<ActionResult> RemoveCaregiver(string caregiverEmail)
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
        {
            _logger.LogError("User claim is null or empty.");
            return BadRequest("User claim is not available.");
        }

        Caregiver? caregiver = await _caregiverManager.FindByEmailAsync(caregiverEmail);
        if (caregiver == null)
        {
            _logger.LogError("Caregiver not found.");
            return BadRequest("Caregiver not found.");
        }
        Elder? elder = await _elderManager.FindByEmailAsync(userClaim.Value);
        if(elder == null)
        {
            _logger.LogError("Elder not found.");
            return NotFound();
        }

        caregiver.Elders.Remove(elder);
        try
        {
            await _caregiverManager.UpdateAsync(caregiver);
            _logger.LogInformation("{elder.Email} removed from Caregiver {caregiver.name}.", elder.Email, caregiver.Name);
            return Ok();
        }
        catch
        {
            _logger.LogError("Failed to update caregiver.");
            return BadRequest();
        }
    }

    [HttpGet("users/getElders")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult<List<Elder>>> GetElders()
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
        {
            _logger.LogError("User claim is null or empty.");
            return BadRequest("User claim is not available.");
        }

        Caregiver? caregiver = await _caregiverManager.FindByEmailAsync(userClaim.Value);
        if (caregiver == null)
        {
            _logger.LogError("Caregiver not found.");
            return BadRequest("Caregiver not found.");
        }
        List<Elder> elders = caregiver.Elders;
        return elders;
    }
    
    [HttpGet("users/arduino")]
    public async Task<ActionResult<List<string?>>> GetUnusedArduino()
    {
        //Get a list of all Max30102 address that has not an elder associated with it
        List<string> address = await _dbContext.MAX30102Data.Select(a => a.Address).Distinct().ToListAsync();
        List<string?> addressNotAssociated = address.Except(_elderManager.Users.Select(e => e.Arduino)).ToList();
        
        return addressNotAssociated;
    }
    
    [HttpPost("users/arduino")]
    public async Task<ActionResult> SetArduino(string email, string address)
    {
        Elder? elder = await _elderManager.FindByEmailAsync(email);
        if (elder == null)
        {
            _logger.LogError("Elder not found.");
            return NotFound();
        }

        elder.MAX30102Data = await _dbContext.MAX30102Data.Where(m => m.Address == address).ToListAsync();
        elder.GPSData = await _dbContext.GPSData.Where(m => m.Address == address).ToListAsync();
        try
        {
            await _elderManager.UpdateAsync(elder);
            _logger.LogInformation("Arduino address set for {elder.Email}.", elder.Email);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update elder.");
            return BadRequest();
        }
    }
}
