using System.Security.Claims;
using HealthDevice.Data;
using HealthDevice.DTO;
using HealthDevice.Models;
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
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;
    private readonly IGeoService _geoService;
    private readonly IRepository<Elder> _elderRepository;
    private readonly IRepository<Caregiver> _caregiverRepository;
    private readonly ApplicationDbContext _dbContext;



    public UserController
    (
        UserManager<Elder> elderManager,
        UserManager<Caregiver> caregiverManager,
        IUserService userService,
        ILogger<UserController> logger,
        IGeoService geoService,
        IRepository<Elder> elderRepository,
        IRepository<Caregiver> caregiverRepository,
        ApplicationDbContext dbContext
    )
    {
        _elderManager = elderManager;
        _caregiverManager = caregiverManager;
        _userService = userService;
        _logger = logger;
        _geoService = geoService;
        _elderRepository = elderRepository;
        _caregiverRepository = caregiverRepository;
        _dbContext = dbContext;
    }


    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDTO>> Login(UserLoginDTO userLoginDto)
    {
        if (string.IsNullOrEmpty(userLoginDto.Email) || string.IsNullOrEmpty(userLoginDto.Password) ||
            !userLoginDto.Email.Contains('@') || userLoginDto.Password.Length < 6)
            return BadRequest("Email and password are in wrong format."); 
        
        _logger.LogInformation("Login attempt for Email: {Email}", userLoginDto.Email);
        return await _userService.HandleLogin(userLoginDto, HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown");
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult> Register(UserRegisterDTO userRegisterDto)
    {
        _logger.LogInformation("Registration attempt for Email: {Email} with Role: {Role}", userRegisterDto.Email, userRegisterDto.Role);
        
        if (string.IsNullOrEmpty(userRegisterDto.Email) || string.IsNullOrEmpty(userRegisterDto.Password) ||
            !userRegisterDto.Email.Contains('@') || userRegisterDto.Password.Length < 6)
            return BadRequest("Email and password are in wrong format."); 
        
        if (userRegisterDto.Role != Roles.Elder && userRegisterDto.Role != Roles.Caregiver)
            return BadRequest("Invalid Role.");
        
        if (userRegisterDto.Role == Roles.Elder && (userRegisterDto.Latitude == null || userRegisterDto.Longitude == null))
            return BadRequest("Elder registration requires Latitude and Longitude.");
        
        if (userRegisterDto is { Role: Roles.Caregiver, Latitude: not null, Longitude: not null })
            return BadRequest("Caregiver registration should not include Latitude and Longitude.");
        
        return userRegisterDto.Role == Roles.Elder
            ? await _userService.HandleRegister(_elderManager, userRegisterDto,
                                                new Elder
                                                {
                                                    Name = userRegisterDto.Name,
                                                    Email = userRegisterDto.Email,
                                                    UserName = userRegisterDto.Email,
                                                    Latitude = (double)userRegisterDto.Latitude,
                                                    Longitude = (double)userRegisterDto.Longitude,
                                                    OutOfPerimeter = false
                                                }, HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown")
            : await _userService.HandleRegister(_caregiverManager, userRegisterDto,
                                                new Caregiver
                                                {
                                                    Name = userRegisterDto.Name,
                                                    Email = userRegisterDto.Email,
                                                    UserName = userRegisterDto.Email,
                                                    Elders = new List<Elder>()
                                                }, HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown");
    }

    [HttpGet("elder")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult<List<GetElderDTO>>> GetUsers()
    {
        _logger.LogInformation("Fetching all Elders");
        return await _elderRepository.Query().Select(e => new GetElderDTO
        {
            Email = e.Email,
            Name = e.Name,
            Role = Roles.Elder
        }).ToListAsync();
    }

    [HttpPost("users/elder")]
    [Authorize(Roles = "Elder")]
    public async Task<ActionResult> PutCaregiver(string caregiverEmail)
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            return BadRequest("User claim is not available.");

        Caregiver? caregiver = await _caregiverRepository.Query()
            .Include(c => c.Invites)
            .FirstOrDefaultAsync(c => c.Email == caregiverEmail);

        if (caregiver == null)
            return BadRequest("Caregiver not found.");

        Elder? elder = await _elderRepository.Query().FirstOrDefaultAsync(e => e.Email == userClaim.Value);
        if (elder == null)
            return NotFound("Elder not found.");


        if (caregiver.Invites != null && caregiver.Invites.Any(e => e.Id == elder.Id))
        {
            _logger.LogInformation("Elder {elder.Email} is already invited by Caregiver {caregiver.Name}.", elder.Email, caregiver.Name);
            return BadRequest("Elder is already invited by this caregiver.");
        }

        caregiver.Invites ??= new List<Elder>();
        caregiver.Invites.Add(elder);

        try
        {
            _dbContext.Update(caregiver);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Elder {elder.Email} sent an invite to Caregiver {caregiver.Name}.", elder.Email, caregiver.Name);
            return Ok("Caregiver invited successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to invite caregiver {caregiver} to elder {elder}.", caregiver.Name, elder.Name);
            return BadRequest("Failed to invite caregiver.");
        }
    }

    [HttpDelete("users/elder/removeCaregiver")]
    [Authorize(Roles = "Elder")]
    public async Task<ActionResult> RemoveCaregiver(string caregiverEmail)
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            return BadRequest("User claim is not available.");

        Caregiver? caregiver = await _caregiverRepository.Query()
            .Include(c => c.Elders) // Ensure Elders collection is included
            .FirstOrDefaultAsync(c => c.Email == caregiverEmail);

        if (caregiver == null)
            return BadRequest("Caregiver not found.");

        Elder? elder = await _elderRepository.Query()
            .FirstOrDefaultAsync(e => e.Email == userClaim.Value);

        if (elder == null)
            return NotFound("Elder not found.");
        
        try
        {
            elder.CaregiverId = null;
            _dbContext.Entry(elder).State = EntityState.Modified;
            _dbContext.Update(elder);
            await _dbContext.SaveChangesAsync(); 
            _logger.LogInformation("{elder.Email} removed from Caregiver {caregiver.Name}.", elder.Email, caregiver.Name);
            return Ok("Caregiver removed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove caregiver {caregiver} from elder {elder}.", caregiver.Name, elder.Name);
            return BadRequest("Failed to remove caregiver.");
        }
    }

    [HttpDelete("users/caregiver/removeFromElder")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult> RemoveFromElder(string elderEmail)
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            return BadRequest("User claim is not available.");

        Caregiver? caregiver = await _caregiverRepository.Query()
            .Include(c => c.Elders) // Ensure Elders collection is included
            .FirstOrDefaultAsync(c => c.Email == userClaim.Value);

        if (caregiver == null)
            return BadRequest("Caregiver not found.");

        Elder? elder = await _elderRepository.Query()
            .FirstOrDefaultAsync(e => e.Email == elderEmail);

        if (elder == null)
            return NotFound("Elder not found.");

        try
        {
            elder.CaregiverId = null;
            _dbContext.Entry(elder).State = EntityState.Modified;
            _dbContext.Update(elder);
            await _dbContext.SaveChangesAsync();   // Persist changes to the database
            _logger.LogInformation("{ElderEmail} removed from Caregiver {CaregiverEmail}.", elderEmail, caregiver.Email);
            return Ok("Elder removed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove elder {elder} from caregiver {caregiver}.", elder.Name, caregiver.Name);
            return BadRequest("Failed to remove elder from caregiver.");
        }
    }

    [HttpGet("users/getElders")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult<List<GetElderDTO>>> GetElders()
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            return BadRequest("User claim is not available.");
        
        Caregiver? caregiver = await _caregiverRepository.Query()
            .Include(c => c.Elders)
            .FirstOrDefaultAsync(c => c.Email == userClaim.Value);
        if (caregiver == null || caregiver.Elders == null)
            return BadRequest("Caregiver not found or has no elders.");
        
        List<Elder> elders = caregiver.Elders;
        _logger.LogInformation("Caregiver {caregiver} has {Count} elders.", caregiver.Name, elders.Count);
        return elders.Select(e => new GetElderDTO
        {
            Name = e.Name,
            Email = e.Email,
            Role = Roles.Elder
        }).ToList();
        
    }

    [HttpGet("users/arduino")]
    public async Task<ActionResult<List<ArduinoInfoDTO>>> GetUnusedArduino()
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            return BadRequest("User claim is not available.");
        
        Elder? elder = await _elderRepository.Query().FirstOrDefaultAsync(m => m.Email == userClaim.Value);
        if (elder == null)
            return NotFound();
        
        return await _userService.GetUnusedArduino(elder);
    }

    [HttpPost("users/arduino")]
    public async Task<ActionResult> SetArduino(string address)
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            return BadRequest("User claim is not available.");

        Elder? elder = await _elderRepository.Query().FirstOrDefaultAsync(m => m.Email == userClaim.Value);
        if (elder == null || string.IsNullOrEmpty(address))
            return BadRequest("Couldnt find elder");
        
        _logger.LogInformation("Setting Arduino address for elder {elder.Email} to {address}.", elder.Email, address);
        
        try
        {
            elder.MacAddress = address;
            await _elderRepository.Update(elder);
            _logger.LogInformation("Arduino address set for {elder.Email}.", elder.Email);
            return Ok("Arduino address set successfully.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to set Arduino address for elder {elder.Email}.", elder.Email);
            return BadRequest("Failed to set Arduino address.");
        }
    }

    [HttpDelete("users/arduino")]
    public async Task<ActionResult> RemoveArduino()
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            return BadRequest("User claim is not available.");

        Elder? elder = await _elderRepository.Query().FirstOrDefaultAsync(m => m.Email == userClaim.Value);
        if (elder == null || string.IsNullOrEmpty(elder.MacAddress))
            return BadRequest("Arduino address is already null.");
        
        _logger.LogInformation("Removing Arduino address for elder {elder.Email}.", elder.Email);
   
        try
        {
            elder.MacAddress = null;
            await _elderRepository.Update(elder);
            _logger.LogInformation("Arduino address removed for {elder.Email}.", elder.Email);
            return Ok("Arduino address removed successfully.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to remove Arduino address for elder {elder.Email}.", elder.Email);
            return BadRequest("Failed to remove Arduino address.");
        }
    }

    [HttpGet("connected")]
    public async Task<ActionResult<bool>> IsConnected(string elderEmail)
    {
        _logger.LogInformation("Connected to elder {elder.Email}.", elderEmail);
        Elder? elder = await _elderRepository.Query().FirstOrDefaultAsync(m => m.Email == elderEmail);
        if (!(elder == null || string.IsNullOrEmpty(elder.MacAddress))) return true;
        
        return NotFound("Elder not found.");
    }

    [HttpPost("elder/address")]
    public async Task<ActionResult> AddAddress(AddressDTO address, string elderEmail)
    {
        Elder? elder = await _elderRepository.Query().FirstOrDefaultAsync(m => m.Email == elderEmail);
        if (elder == null)
            return NotFound("Elder not found.");
        
        if (string.IsNullOrEmpty(address.Street) || string.IsNullOrEmpty(address.City))
            return BadRequest("Address cannot be null or empty.");

        var result = await _geoService.GetCoordinatesFromAddress(address.Street, address.City);
        if (result == null)
            return BadRequest("Failed to get coordinates from address.");
        
        try
        {
            elder.Latitude = result.Latitude;
            elder.Longitude = result.Longitude;
            await _elderRepository.Update(elder);
            _logger.LogInformation("Adding address for elder {elder.Email} to {address}.", elder.Email, address);
            return Ok("Address added successfully.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to add address {elderEmail}.", elder.Email);
            return BadRequest("Failed to add address.");
        }
    }

    [HttpGet("caregiver/invites")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult<List<GetElderDTO>>> GetInvites()
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            return BadRequest("User claim is not available.");
        
        Caregiver? caregiver = await _caregiverRepository.Query()
            .Include(c => c.Invites)
            .FirstOrDefaultAsync(c => c.Email == userClaim.Value);
        if (caregiver == null || caregiver.Invites == null)
        {
            return BadRequest("Caregiver has no invites.");
        }
        
        _logger.LogInformation("Caregiver has {Count} invites.", caregiver.Invites.Count);
        return caregiver.Invites.Select(elder => new GetElderDTO { Name = elder.Name, Email = elder.Email, Role = Roles.Elder }).ToList();
    }

    [HttpPost("caregiver/invites/accept")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult> AcceptInvite(string elderEmail)
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            return BadRequest("User claim is not available.");
        
        Caregiver? caregiver = await _caregiverRepository.Query()
            .Include(c => c.Invites)
            .Include(c => c.Elders)
            .FirstOrDefaultAsync(c => c.Email == userClaim.Value);
        if (caregiver?.Invites == null || caregiver.Invites.Count == 0)
            return BadRequest("No invites found.");

        Elder? elder = caregiver.Invites.FirstOrDefault(m => m.Email == elderEmail);
        if (elder == null)
            return NotFound("Elder not found.");

        try
        {
            elder.CaregiverId = caregiver.Id;
            elder.InvitedCaregiverId = null;
            _dbContext.Entry(elder).State = EntityState.Modified;
            _dbContext.Update(elder);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Caregiver {caregiver.Name} accepted invite from Elder {elder.Email}.", caregiver.Name, elder.Email);
            return Ok("Invite accepted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update caregiver.");
            return BadRequest("Failed to update caregiver.");
        }
    }

    [HttpGet("users/elder/Arduino")]
    [Authorize(Roles = "Elder")]
    public async Task<ActionResult<string>> GetElderArduino()
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            return BadRequest("User claim is not available.");

        Elder? elder = await _elderRepository.Query().FirstOrDefaultAsync(m => m.Email == userClaim.Value);
        if (elder?.MacAddress == null)
            return NotFound("No mac address found.");
        
        return elder.MacAddress;
    }

    [HttpGet("users/elder/caregiver")]
    [Authorize(Roles = "Elder")]
    public async Task<ActionResult<List<CaregiverDTO>>> GetElderCaregiver()
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            return BadRequest("User claim is not available.");

        Elder? elder = await _elderRepository.Query().FirstOrDefaultAsync(m => m.Email == userClaim.Value);
        if (elder?.CaregiverId == null)
            return NotFound("No caregiver found.");

        Caregiver? caregiver = await _caregiverRepository.Query().FirstOrDefaultAsync(m => m.Id == elder.CaregiverId);
        if (caregiver == null)
            return NotFound("No caregiver found.");
        
        return new List<CaregiverDTO>
        {
            new()
            {
                Name = caregiver.Name,
                Email = caregiver.Email
            }
        };
    }

    [HttpGet("renew/token")]
    [Authorize(AuthenticationSchemes = "ExpiredTokenScheme")]
    public async Task<ActionResult<string>> RenewToken()
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            return BadRequest("User claim is not available.");
        
        var elder = await _elderRepository.Query().FirstOrDefaultAsync(m => m.Email == userClaim.Value);
        var caregiver = await _caregiverRepository.Query().FirstOrDefaultAsync(m => m.Email == userClaim.Value);
        
        var expiredClaim = User.Claims.FirstOrDefault(c => c.Type == "exp");

        if (expiredClaim == null)
            return BadRequest("Expiration claim not found.");

        DateTime expTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiredClaim.Value)).DateTime;
        if (expTime > DateTime.UtcNow)
            return BadRequest("Token is not expired yet.");

        if (DateTime.UtcNow <= expTime || expTime <= DateTime.UtcNow + TimeSpan.FromMinutes(5))
        {
            if (caregiver != null)
                return _userService.GenerateJwt(caregiver, "Caregiver");
            if (elder != null)
                return _userService.GenerateJwt(elder, "Elder");
        }
        return BadRequest("Token is not expired yet.");
    }
    
}
