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
    private readonly HealthService _healthService;
    
    public UserController(
        UserManager<Elder> elderManager,
        UserManager<Caregiver> caregiverManager,
        UserService userService,
        ILogger<UserController> logger,
        ApplicationDbContext dbContext,
        GeoService geoService,
        HealthService healthService)
    {
        _elderManager = elderManager;
        _caregiverManager = caregiverManager;
        _userService = userService;
        _logger = logger;
        _dbContext = dbContext;
        _geoService = geoService;
        _healthService = healthService;
    }
    
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDTO>> Login(UserLoginDTO userLoginDto)
    {
        if (string.IsNullOrEmpty(userLoginDto.Email) || string.IsNullOrEmpty(userLoginDto.Password))
        {
            _logger.LogWarning("Login attempt with empty email or password.");
            return BadRequest("Email and password are required.");
        }
        if (!userLoginDto.Email.Contains("@"))
        {
            _logger.LogWarning("Invalid email format: {Email}", userLoginDto.Email);
            return BadRequest("Invalid email format.");
        }
        if (userLoginDto.Password.Length < 6)
        {
            _logger.LogWarning("Password too short: {PasswordLength} characters", userLoginDto.Password.Length);
            return BadRequest("Password must be at least 6 characters long.");
        }
        _logger.LogInformation("Login attempt for email: {Email}", userLoginDto.Email);
        return await _userService.HandleLogin(userLoginDto, HttpContext);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult> Register(UserRegisterDTO userRegisterDto)
    {
        if (string.IsNullOrEmpty(userRegisterDto.Email) || string.IsNullOrEmpty(userRegisterDto.Password))
        {
            _logger.LogWarning("Registration attempt with empty email or password.");
            return BadRequest("Email and password are required.");
        }
        if (!userRegisterDto.Email.Contains("@"))
        {
            _logger.LogWarning("Invalid email format: {Email}", userRegisterDto.Email);
            return BadRequest("Invalid email format.");
        }
        if (userRegisterDto.Password.Length < 6)
        {
            _logger.LogWarning("Password too short: {PasswordLength} characters", userRegisterDto.Password.Length);
            return BadRequest("Password must be at least 6 characters long.");
        }
        if (userRegisterDto.Role != Roles.Elder && userRegisterDto.Role != Roles.Caregiver)
        {
            _logger.LogWarning("Invalid role: {Role}", userRegisterDto.Role);
            return BadRequest("Invalid role.");
        }
        if (userRegisterDto.Role == Roles.Elder && (userRegisterDto.latitude == null || userRegisterDto.longitude == null))
        {
            _logger.LogWarning("Elder registration requires latitude and longitude.");
            return BadRequest("Elder registration requires latitude and longitude.");
        }
        if (userRegisterDto.Role == Roles.Caregiver && userRegisterDto.latitude != null && userRegisterDto.longitude != null)
        {
            _logger.LogWarning("Caregiver registration should not include latitude and longitude.");
            return BadRequest("Caregiver registration should not include latitude and longitude.");
        }
        _logger.LogInformation("Registration attempt for email: {Email} with role: {Role}", userRegisterDto.Email, userRegisterDto.Role);
        return userRegisterDto.Role == Roles.Elder 
            ? await _userService.HandleRegister(_elderManager, userRegisterDto, 
                                                new Elder
                                                {
                                                    Name = userRegisterDto.Name,
                                                    Email = userRegisterDto.Email, 
                                                    UserName = userRegisterDto.Email, 
                                                    latitude = (double)userRegisterDto.latitude,
                                                    longitude = (double)userRegisterDto.longitude
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
    public async Task<ActionResult<List<GetElderDTO>>> GetUsers()
    {
        _logger.LogInformation("Fetching all elders.");
        List<Elder> elders = await _elderManager.Users.ToListAsync();
        _logger.LogInformation("Fetched {Count} elders.", elders.Count);
        return elders.Select(e => new GetElderDTO
        {
            Email = e.Email,
            Name = e.Name,
        }).ToList();
    }


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
        _logger.LogInformation("Caregiver found. {caregiver}", caregiver);
        Elder? elder = await _elderManager.FindByEmailAsync(userClaim.Value);
        if (elder == null)
        {
            _logger.LogError("Elder not found.");
            return NotFound("Elder not found.");
        }

        _logger.LogInformation("Elder found. {elder}", elder);

        // Add the elder to the caregiver's Elders collection
        caregiver.Invites ??= new List<Elder>();
        caregiver.Invites.Add(elder);
        _logger.LogInformation("Elder {elder.Email} added to Caregiver {caregiver.Name}.", elder.Email, caregiver.Name);
        try
        {
            // Save changes explicitly
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("{elder.Email} added to Caregiver {caregiver.Name}.", elder.Email, caregiver.Name);
            return Ok("Caregiver invited successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update caregiver.");
            return BadRequest("Failed to invite caregiver.");
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
        _logger.LogInformation("Caregiver found. {caregiver}", caregiver);
        Elder? elder = await _elderManager.FindByEmailAsync(userClaim.Value);
        if(elder == null)
        {
            _logger.LogError("Elder not found.");
            return NotFound("Elder not found.");
        }
        _logger.LogInformation("Elder found. {elder}", elder);

        if (caregiver.Elders != null) caregiver.Elders.Remove(elder);
        try
        {
            await _caregiverManager.UpdateAsync(caregiver);
            _logger.LogInformation("{elder.Email} removed from Caregiver {caregiver.name}.", elder.Email, caregiver.Name);
            return Ok("Caregiver removed successfully.");
        }
        catch
        {
            _logger.LogError("Failed to update caregiver.");
            return BadRequest("Failed to remove caregiver.");
        }
    }

    [HttpGet("users/getElders")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult<List<GetElderDTO>>> GetElders()
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
        _logger.LogInformation("Caregiver found. {caregiver}", caregiver);
        
        if (caregiver.Elders != null)
        {
            List<Elder> elders = caregiver.Elders;
            _logger.LogInformation("Caregiver has {Count} elders.", elders.Count);
            List<GetElderDTO> elderDTOs = elders.Select(e => new GetElderDTO
            {
                Name = e.Name,
                Email = e.Email,
            }).ToList();
            return elderDTOs;
        }
        _logger.LogError("Caregiver has no elders.");
        return BadRequest("Caregiver has no elders.");
    }
    
    [HttpGet("users/arduino")]
    public async Task<ActionResult<List<ArduinoInfo>>> GetUnusedArduino()
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
        {
            _logger.LogError("User claim is null or empty.");
            return BadRequest("User claim is not available.");
        }
        Elder? elder = await _elderManager.FindByEmailAsync(userClaim.Value);
        if (elder == null)
        {
            _logger.LogError("Elder not found.");
            return NotFound();
        }
        if (string.IsNullOrEmpty(elder.Arduino))
        {
            _logger.LogError("Elder has no Arduino address.");
            return BadRequest("Elder has no Arduino address.");
        }
        _logger.LogInformation("Elder found. {elder}", elder);
        Location elderLocation = new Location
        {
            Latitude = elder.latitude,
            Longitude = elder.longitude
        };
        _logger.LogInformation("Elder location: {elderLocation}", elderLocation);
        List<Elder> elders = _elderManager.Users.ToList();
        _logger.LogInformation("Elders count: {elders}", elders.Count);
        List<GPS> gpsData = _dbContext.GPSData.ToList();
        _logger.LogInformation("GPS data count: {gpsData}", gpsData.Count);
        var filteredGpsData = gpsData.Where(g => elders.All(e => e.Arduino != g.Address)).ToList();
        _logger.LogInformation("Filtered GPS data count: {filteredGpsData}", filteredGpsData.Count);
        List<ArduinoInfo> addressNotAssociated = new List<ArduinoInfo>();
        foreach (GPS gps in filteredGpsData)
        {
            string GpsAddress = await _geoService.GetAddressFromCoordinates(gps.Latitude, gps.Longitude);
            double distance = _geoService.CalculateDistance(new Location{Latitude = gps.Latitude, Longitude = gps.Longitude}, elderLocation);
            int minutesSinceActivity = (int)(DateTime.UtcNow - gps.Timestamp).TotalMinutes;
            if (distance < 0.5)
            {
                _logger.LogInformation("Distance: {distance} km, Address: {GpsAddress}, Minutes since activity: {minutesSinceActivity}", distance, GpsAddress, minutesSinceActivity);
                if (gps.Address != null)
                {
                    ArduinoInfo arduinoInfo = new ArduinoInfo
                    {
                        Id = gps.Id,
                        MacAddress = gps.Address,
                        Address = GpsAddress,
                        Distance = distance,
                        lastActivity = minutesSinceActivity
                    };
                    addressNotAssociated.Add(arduinoInfo);
                }
            }
        }
        _logger.LogInformation("Address not associated count: {addressNotAssociated}", addressNotAssociated.Count);
        return addressNotAssociated.Count != 0 ? addressNotAssociated : [];
    }
    
    [HttpPost("users/arduino")]
    public async Task<ActionResult> SetArduino(string address)
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
        {
            _logger.LogError("User claim is null or empty.");
            return BadRequest("User claim is not available.");
        }
        
        Elder? elder = await _elderManager.FindByEmailAsync(userClaim.Value);
        if (elder == null)
        {
            _logger.LogError("Elder not found.");
            return NotFound();
        }
        if (string.IsNullOrEmpty(address))
        {
            _logger.LogError("Arduino address is null or empty.");
            return BadRequest("Arduino address cannot be null or empty.");
        }
        _logger.LogInformation("Setting Arduino address for elder {elder.Email} to {address}.", elder.Email, address);
        elder.Arduino = address;
        try
        {
            await _elderManager.UpdateAsync(elder);
            _logger.LogInformation("Arduino address set for {elder.Email}.", elder.Email);
            return Ok("Arduino address set successfully.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to set Arduino address for elder {elder.Email}.", elder.Email);
            return BadRequest("Failed to set Arduino address.");
        }
    }

    [HttpGet("connected")]
    public async Task<ActionResult<bool>> IsConnected(string elderEmail)
    {
        Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
        if (elder == null)
        {
            _logger.LogError("Elder not found.");
            return NotFound("Elder not found.");
        }
        if (string.IsNullOrEmpty(elder.Arduino))
        {
            _logger.LogError("Elder has no Arduino address.");
            return BadRequest("Elder has no Arduino address.");
        }
        _logger.LogInformation("Connected to elder {elder.Email}.", elder.Email);

        return true;
    }

    [HttpPost("elder/address")]
    public async Task<ActionResult> AddAddress(Address address, string elderEmail)
    {
        Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
        if (elder == null)
        {
            _logger.LogError("Elder not found.");
            return NotFound("Elder not found.");
        }
        _logger.LogInformation("Elder found. {elder}", elder);
        if (string.IsNullOrEmpty(address.Street) || string.IsNullOrEmpty(address.City))
        {
            _logger.LogError("Address is null or empty.");
            return BadRequest("Address cannot be null or empty.");
        }

        var result = await _geoService.GetCoordinatesFromAddress(address.Street, address.City);
        _logger.LogInformation("Coordinates retrieved: {result}", result);
        if (result == null)
        {
            _logger.LogError("Failed to get coordinates from address.");
            return BadRequest("Failed to get coordinates from address.");
        }
        elder.latitude = result.Latitude;
        elder.longitude = result.Longitude;
        _logger.LogInformation("Setting address for elder {elder.Email} to {address}.", elder.Email, address);
        try
        {
            await _elderManager.UpdateAsync(elder);
            _logger.LogInformation("Address added for elder {elder.Email}.", elder.Email);
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
        _logger.LogInformation("Caregiver found. {caregiver}", caregiver);
        
        if (caregiver.Invites != null)
        {
            _logger.LogInformation("Caregiver has {Count} invites.", caregiver.Invites.Count);
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
        _logger.LogInformation("Caregiver found. {caregiver}", caregiver);
        Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
        if (elder == null)
        {
            _logger.LogError("Elder not found.");
            return NotFound();
        }
        _logger.LogInformation("Elder found. {elder}", elder);
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
            return Ok("Invite accepted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update caregiver.");
            return BadRequest("Failed to update caregiver.");
        }
    }
}
