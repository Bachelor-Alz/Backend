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
    private readonly GeoService _geoService;
    
    public UserController(
        UserManager<Elder> elderManager,
        UserManager<Caregiver> caregiverManager,
        UserService userService,
        ILogger<UserController> logger,
        ApplicationDbContext dbContext,
        GeoService geoService)
    {
        _elderManager = elderManager;
        _caregiverManager = caregiverManager;
        _userService = userService;
        _logger = logger;
        _dbContext = dbContext;
        _geoService = geoService;
    }
    
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDTO>> Login(UserLoginDTO userLoginDto)
    {
        return await _userService.HandleLogin(userLoginDto, HttpContext);
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

        // Use Include to ensure Elders are loaded with the Caregiver
        Caregiver? caregiver = await _caregiverManager.Users
            .Include(c => c.Invites)
            .FirstOrDefaultAsync(c => c.Email == caregiverEmail);

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

        _logger.LogInformation("Caregiver found. {caregiver}", caregiver);

        // Add the elder to the caregiver's Elders collection
        caregiver.Invites ??= new List<Elder>();
        caregiver.Invites.Add(elder);

        try
        {
            // Save changes explicitly
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("{elder.Email} added to Caregiver {caregiver.Name}.", elder.Email, caregiver.Name);
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

        if (caregiver.Elders != null) caregiver.Elders.Remove(elder);
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

        // Include Elders when retrieving the Caregiver
        Caregiver? caregiver = await _caregiverManager.Users
            .Include(c => c.Elders)
            .FirstOrDefaultAsync(c => c.Email == userClaim.Value);
        if (caregiver == null)
        {
            _logger.LogError("Caregiver not found.");
            return BadRequest("Caregiver not found.");
        }
        
        if (caregiver.Elders != null)
        {
            List<Elder> elders = caregiver.Elders;
            return elders;
        }
        _logger.LogError("Caregiver has no elders.");
        return BadRequest("Caregiver has no elders.");
    }
    
    [HttpGet("users/arduino")]
    public async Task<ActionResult<List<string>>> GetUnusedArduino()
    {
        List<string?> address = await _dbContext.MAX30102Data
            .Select(a => a.Address)
            .Distinct()
            .ToListAsync();

        List<string> addressNotAssociated = address
            .Where(a => a != null)
            .Select(a => a!)
            .Except(_elderManager.Users.Select(e => e.Arduino ?? string.Empty))
            .ToList();

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
        elder.Arduino = address;
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

    [HttpGet("connected")]
    public async Task<ActionResult<bool>> IsConnected(string elderEmail)
    {
        Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
        if (elder == null)
        {
            _logger.LogError("Elder not found.");
            return NotFound();
        }

        if (elder.Arduino == null)
        {
            _logger.LogError("Elder has no Arduino address.");
            return false;
        }

        return true;
    }

    [HttpPost("elder/address")]
    public async Task<ActionResult> AddAddress(Address address, string elderEmail)
    {
        Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
        if (elder == null)
        {
            _logger.LogError("Elder not found.");
            return NotFound();
        }

        var result = await _geoService.GetCoordinatesFromAddress(address.Street, address.City, address.State,
            address.Country, address.ZipCode, null);
        if (result == null)
        {
            _logger.LogError("Failed to get coordinates from address.");
            return BadRequest("Failed to get coordinates from address.");
        }
        elder.latitude = result.Latitude;
        elder.longitude = result.Longitude;

        try
        {
            await _elderManager.UpdateAsync(elder);
            _logger.LogInformation("Address added for elder {elder.Email}.", elder.Email);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to add address {elderEmail}.", elder.Email);
            return BadRequest("Failed to add address.");
        }
    }

    [HttpGet("caregiver/invites")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult<List<Elder>>> GetInvites()
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
        {
            _logger.LogError("User claim is null or empty.");
            return BadRequest("User claim is not available.");
        }

        // Include Elders when retrieving the Caregiver
        Caregiver? caregiver = await _caregiverManager.Users
            .Include(c => c.Invites)
            .FirstOrDefaultAsync(c => c.Email == userClaim.Value);
        if (caregiver == null)
        {
            _logger.LogError("Caregiver not found.");
            return BadRequest("Caregiver not found.");
        }
        
        if (caregiver.Invites != null)
        {
            List<Elder> invites = caregiver.Invites;
            return invites;
        }
        _logger.LogError("Caregiver has no invites.");
        return BadRequest("Caregiver has no invites.");
    }

    [HttpPost("caregiver/invites/accept")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult> AcceptInvite(string elderEmail)
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
        {
            _logger.LogError("User claim is null or empty.");
            return BadRequest("User claim is not available.");
        }
        
        Caregiver? caregiver = await _caregiverManager.Users
            .Include(c => c.Invites)
            .Include(e => e.Elders)
            .FirstOrDefaultAsync(c => c.Email == userClaim.Value);
        if (caregiver == null)
        {
            _logger.LogError("Caregiver not found.");
            return BadRequest("Caregiver not found.");
        }
        Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
        if (elder == null)
        {
            _logger.LogError("Elder not found.");
            return NotFound();
        }
        if (caregiver.Invites != null)
        {
            caregiver.Elders ??= new List<Elder>();
            caregiver.Elders.Add(elder);
            caregiver.Invites.Remove(elder);
        }
        else
        {
            _logger.LogError("Caregiver has no invites.");
            return BadRequest("Caregiver has no invites.");
        }
        try
        {
            await _caregiverManager.UpdateAsync(caregiver);
            _logger.LogInformation("Caregiver {caregiver.Name} accepted invite from Elder {elder.Email}.", caregiver.Name, elder.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update caregiver.");
            return BadRequest("Failed to update caregiver.");
        }
        _logger.LogInformation("Caregiver {caregiver.Name} accepted invite from Elder {elder.Email}.", caregiver.Name, elder.Email);
        return Ok();
    }
}
