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
    
    public UserController(UserManager<Elder> elderManager, UserManager<Caregiver> caregiverManager,
        UserService userService, ILogger<UserController> logger, ApplicationDbContext dbContext)
    {
        _elderManager = elderManager;
        _caregiverManager = caregiverManager;
        _userService = userService;
        _logger = logger;
        _dbContext = dbContext;
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
            ? await _userService.HandleRegister(_elderManager, userRegisterDTO, 
                                                new Elder
                                                {
                                                    name = userRegisterDTO.Name,
                                                    Email = userRegisterDTO.Email, 
                                                    UserName = userRegisterDTO.Email, 
                                                    Max30102Datas = new List<Max30102>(), 
                                                    gpsData = new List<GPS>(),
                                                    location = new Location(),
                                                    perimeter = new Perimeter{location = new Location()},
                                                }, HttpContext)
            : await _userService.HandleRegister(_caregiverManager, userRegisterDTO, 
                                                new Caregiver
                                                {
                                                    name = userRegisterDTO.Name, 
                                                    Email = userRegisterDTO.Email, 
                                                    UserName = userRegisterDTO.Email, 
                                                    elders = new List<Elder>()
                                                }, HttpContext);
    }

    [HttpGet("elder")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult<List<Elder>>> GetUsers() => await _elderManager.Users.ToListAsync();


    [HttpPost("users/elder")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult> PutElder(string ElderEmail)
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

        Elder? elder = await _elderManager.FindByEmailAsync(ElderEmail);
        if (elder == null)
        {
            _logger.LogError("Elder not found.");
            return NotFound("Elder not found.");
        }

        if (caregiver.elders == null)
        {
            caregiver.elders = new List<Elder>();
        }

        caregiver.elders.Add(elder);
        try
        {
            await _caregiverManager.UpdateAsync(caregiver);
            _logger.LogInformation("{elder.Email} added to Caregiver {caregiver.name}.", elder.Email, caregiver.name);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update caregiver.");
            return BadRequest("Failed to update caregiver.");
        }
    }

    [HttpDelete("users/elder")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult> RemoveElder(string ElderEmail)
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
        Elder? elder = await _elderManager.FindByEmailAsync(ElderEmail);
        if(elder == null)
        {
            _logger.LogError("Elder not found.");
            return NotFound();
        }

        caregiver.elders.Remove(elder);
        try
        {
            await _caregiverManager.UpdateAsync(caregiver);
            _logger.LogInformation("{elder.Email} removed from Caregiver {caregiver.name}.", elder.Email, caregiver.name);
            return Ok();
        }
        catch
        {
            _logger.LogError("Failed to update caregiver.");
            return BadRequest();
        }
    }
    
    [HttpGet("users/arduino")]
    public async Task<ActionResult<List<string>>> GetUnusedArduino()
    {
        //Get a list of all Max30102 address that has not an elder associated with it
        List<string> Address = await _dbContext.Max30102Data.Select(a => a.Address).Distinct().ToListAsync();
        List<string> AddressNotAssociated = Address.Except(_elderManager.Users.Select(e => e.arduino)).ToList();
        
        return AddressNotAssociated;
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

        elder.Max30102Datas = await _dbContext.Max30102Data.Where(m => m.Address == address).ToListAsync();
        elder.gpsData = await _dbContext.GpsData.Where(m => m.Address == address).ToListAsync();
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
