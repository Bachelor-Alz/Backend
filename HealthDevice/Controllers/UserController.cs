using System.Net;
using System.Security.Claims;
using HealthDevice.Data;
using HealthDevice.DTO;
using HealthDevice.Models;
using HealthDevice.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
    private readonly IRepositoryFactory _repositoryFactory;
    private readonly ApplicationDbContext _dbContext;
    
    public UserController(
        UserManager<Elder> elderManager,
        UserManager<Caregiver> caregiverManager,
        IUserService userService,
        ILogger<UserController> logger,
        IGeoService geoService,
        IRepositoryFactory repositoryFactory,
        ApplicationDbContext dbContext)
    {
        _elderManager = elderManager;
        _caregiverManager = caregiverManager;
        _userService = userService;
        _logger = logger;
        _geoService = geoService;
        _repositoryFactory = repositoryFactory;
        _dbContext = dbContext;
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
        if (!userLoginDto.Email.Contains('@'))
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
        string ipAddress = HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
        return await _userService.HandleLogin(userLoginDto, ipAddress);
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
        if (!userRegisterDto.Email.Contains('@'))
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
        if (userRegisterDto is { Role: Roles.Caregiver, latitude: not null, longitude: not null })
        {
            _logger.LogWarning("Caregiver registration should not include latitude and longitude.");
            return BadRequest("Caregiver registration should not include latitude and longitude.");
        }
        _logger.LogInformation("Registration attempt for email: {Email} with role: {Role}", userRegisterDto.Email, userRegisterDto.Role);
        string ipAddress = HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
        return userRegisterDto.Role == Roles.Elder 
            ? await _userService.HandleRegister(_elderManager, userRegisterDto, 
                                                new Elder
                                                {
                                                    Name = userRegisterDto.Name,
                                                    Email = userRegisterDto.Email, 
                                                    UserName = userRegisterDto.Email, 
                                                    latitude = (double)userRegisterDto.latitude,
                                                    longitude = (double)userRegisterDto.longitude,
                                                    outOfPerimeter = false
                                                }, ipAddress)
            : await _userService.HandleRegister(_caregiverManager, userRegisterDto, 
                                                new Caregiver
                                                {
                                                    Name = userRegisterDto.Name, 
                                                    Email = userRegisterDto.Email, 
                                                    UserName = userRegisterDto.Email, 
                                                    Elders = new List<Elder>()
                                                }, ipAddress);
    }

    [HttpGet("elder")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult<List<GetElderDTO>>> GetUsers()
    {
        IRepository<Elder> elderRepository = _repositoryFactory.GetRepository<Elder>();
        _logger.LogInformation("Fetching all elders.");
        List<Elder> elders = await elderRepository.Query().ToListAsync();
        _logger.LogInformation("Fetched {Count} elders.", elders.Count);
        return elders.Select(e => new GetElderDTO
        {
            Email = e.Email,
            Name = e.Name,
            role = Roles.Elder
        }).ToList();
    }

 [HttpPost("users/elder")]
[Authorize(Roles = "Elder")]
public async Task<ActionResult> PutCaregiver(string caregiverEmail)
{
    IRepository<Caregiver> caregiverRepository = _repositoryFactory.GetRepository<Caregiver>();
    IRepository<Elder> elderRepository = _repositoryFactory.GetRepository<Elder>();

    Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
    {
        _logger.LogError("User claim is null or empty.");
        return BadRequest("User claim is not available.");
    }

    Caregiver? caregiver = await caregiverRepository.Query()
        .Include(c => c.Invites)
        .FirstOrDefaultAsync(c => c.Email == caregiverEmail);

    if (caregiver == null)
    {
        _logger.LogError("Caregiver not found.");
        return BadRequest("Caregiver not found.");
    }

    Elder? elder = await elderRepository.Query().FirstOrDefaultAsync(e => e.Email == userClaim.Value);
    if (elder == null)
    {
        _logger.LogError("Elder not found.");
        return NotFound("Elder not found.");
    }
    

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
        _logger.LogInformation("Elder {elder.Email} added to Caregiver {caregiver.Name}.", elder.Email, caregiver.Name);
        return Ok("Caregiver invited successfully.");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to update caregiver.");
        return BadRequest("Failed to invite caregiver.");
    }
}

    [HttpDelete("users/elder/removeCaregiver")]
[Authorize(Roles = "Elder")]
public async Task<ActionResult> RemoveCaregiver(string caregiverEmail)
{
    IRepository<Caregiver> caregiverRepository = _repositoryFactory.GetRepository<Caregiver>();
    IRepository<Elder> elderRepository = _repositoryFactory.GetRepository<Elder>();

    Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
    {
        _logger.LogError("User claim is null or empty.");
        return BadRequest("User claim is not available.");
    }

    Caregiver? caregiver = await caregiverRepository.Query()
        .Include(c => c.Elders) // Ensure Elders collection is included
        .FirstOrDefaultAsync(c => c.Email == caregiverEmail);

    if (caregiver == null)
    {
        _logger.LogError("Caregiver not found.");
        return BadRequest("Caregiver not found.");
    }

    Elder? elder = await elderRepository.Query()
        .FirstOrDefaultAsync(e => e.Email == userClaim.Value);

    if (elder == null)
    {
        _logger.LogError("Elder not found.");
        return NotFound("Elder not found.");
    }

// Remove the relationship explicitly
    if (elder.CaregiverId != null)
    {
        elder.CaregiverId = null;
        _dbContext.Entry(elder).State = EntityState.Modified;
        _dbContext.Update(elder);
    }
    else
    {
        _logger.LogError("Elder {ElderEmail} not assigned to Caregiver {CaregiverEmail}.", elder.Email, caregiver.Email);
        return BadRequest("Elder not assigned to this caregiver.");
    }

    try
    {
        await _dbContext.SaveChangesAsync();   // Persist changes to the database
        _logger.LogInformation("{elder.Email} removed from Caregiver {caregiver.Name}.", elder.Email, caregiver.Name);
        return Ok("Caregiver removed successfully.");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to remove caregiver.");
        return BadRequest("Failed to remove caregiver.");
    }
}

    [HttpDelete("users/caregiver/removeFromElder")]
[Authorize(Roles = "Caregiver")]
public async Task<ActionResult> RemoveFromElder(string elderEmail)
{
    IRepository<Caregiver> caregiverRepository = _repositoryFactory.GetRepository<Caregiver>();
    IRepository<Elder> elderRepository = _repositoryFactory.GetRepository<Elder>();

    Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
    {
        _logger.LogError("User claim is null or empty.");
        return BadRequest("User claim is not available.");
    }

    Caregiver? caregiver = await caregiverRepository.Query()
        .Include(c => c.Elders) // Ensure Elders collection is included
        .FirstOrDefaultAsync(c => c.Email == userClaim.Value);

    if (caregiver == null)
    {
        _logger.LogError("Caregiver not found.");
        return BadRequest("Caregiver not found.");
    }

    Elder? elder = await elderRepository.Query()
        .FirstOrDefaultAsync(e => e.Email == elderEmail);

    if (elder == null)
    {
        _logger.LogError("Elder not found.");
        return NotFound("Elder not found.");
    }

    // Remove the relationship explicitly
    if (elder.CaregiverId != null)
    {
        elder.CaregiverId = null;
        _dbContext.Entry(elder).State = EntityState.Modified;
        _dbContext.Update(elder);
    }
    else
    {
        _logger.LogError("Elder {ElderEmail} not assigned to Caregiver {CaregiverEmail}.", elderEmail, caregiver.Email);
        return BadRequest("Elder not assigned to this caregiver.");
    }

    try
    {
        await _dbContext.SaveChangesAsync();   // Persist changes to the database
        _logger.LogInformation("{ElderEmail} removed from Caregiver {CaregiverEmail}.", elderEmail, caregiver.Email);
        return Ok("Elder removed successfully.");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to remove elder from caregiver.");
        return BadRequest("Failed to remove elder from caregiver.");
    }
}

    [HttpGet("users/getElders")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult<List<GetElderDTO>>> GetElders()
    {
        IRepository<Caregiver> caregiverRepository = _repositoryFactory.GetRepository<Caregiver>();
        
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
        {
            _logger.LogError("User claim is null or empty.");
            return BadRequest("User claim is not available.");
        }

        // Include Elders when retrieving the Caregiver
        Caregiver? caregiver = await caregiverRepository.Query()
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
                role = Roles.Elder
            }).ToList();
            return elderDTOs;
        }
        _logger.LogError("Caregiver has no elders.");
        return BadRequest("Caregiver has no elders.");
    }
    
    [HttpGet("users/arduino")]
    public async Task<ActionResult<List<ArduinoInfo>>> GetUnusedArduino()
    {
        IRepository<Elder> elderRepository = _repositoryFactory.GetRepository<Elder>();
        IRepository<GPSData> gpsRepository = _repositoryFactory.GetRepository<GPSData>();
        
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
        {
            _logger.LogError("User claim is null or empty.");
            return BadRequest("User claim is not available.");
        }
        Elder? elder = await elderRepository.Query().FirstOrDefaultAsync(m => m.Email == userClaim.Value);
        if (elder == null)
        {
            _logger.LogError("Elder not found.");
            return NotFound();
        }
        _logger.LogInformation("Elder found. {elder}", elder);
        Location elderLocation = new Location
        {
            Latitude = elder.latitude,
            Longitude = elder.longitude
        };
        _logger.LogInformation("Elder location: {elderLocation}", elderLocation);
        List<Elder> elders = await elderRepository.Query().ToListAsync();
        _logger.LogInformation("Elders count: {elders}", elders.Count);
        List<GPSData> gpsData = await gpsRepository.Query().ToListAsync();
        _logger.LogInformation("GPS data count: {gpsData}", gpsData.Count);
        List<GPSData> filteredGpsData = gpsData.Where(g => elders.All(e => e.MacAddress != g.MacAddress)).ToList();
        _logger.LogInformation("Filtered GPS data count: {filteredGpsData}", filteredGpsData.Count);
        List<ArduinoInfo> addressNotAssociated = [];
        foreach (GPSData gps in filteredGpsData)
        {
            string GpsAddress = await _geoService.GetAddressFromCoordinates(gps.Latitude, gps.Longitude);
            float distance = GeoService.CalculateDistance(new Location{Latitude = gps.Latitude, Longitude = gps.Longitude}, elderLocation);
            int minutesSinceActivity = ((int)(DateTime.UtcNow - gps.Timestamp).TotalMinutes)*-1;
            if (!(distance < 0.5)) continue;
            _logger.LogInformation("Distance: {distance} km, Address: {GpsAddress}, Minutes since activity: {minutesSinceActivity}", distance, GpsAddress, minutesSinceActivity);
            if (gps.MacAddress == null) continue;
            ArduinoInfo arduinoInfo = new ArduinoInfo
            {
                Id = gps.Id,
                MacAddress = gps.MacAddress,
                Address = GpsAddress,
                Distance = distance,
                lastActivity = minutesSinceActivity
            };
            addressNotAssociated.Add(arduinoInfo);
        }
        _logger.LogInformation("Address not associated count: {addressNotAssociated}", addressNotAssociated.Count);
        return addressNotAssociated.Count != 0 ? addressNotAssociated : [];
    }
    
    [HttpPost("users/arduino")]
    public async Task<ActionResult> SetArduino(string address)
    {
        IRepository<Elder> elderRepository = _repositoryFactory.GetRepository<Elder>();
        
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
        {
            _logger.LogError("User claim is null or empty.");
            return BadRequest("User claim is not available.");
        }
        
        Elder? elder = await elderRepository.Query().FirstOrDefaultAsync(m => m.Email == userClaim.Value);
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
        elder.MacAddress = address;
        try
        {
            await elderRepository.Update(elder);
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
        IRepository<Elder> elderRepository = _repositoryFactory.GetRepository<Elder>();
        
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
        {
            _logger.LogError("User claim is null or empty.");
            return BadRequest("User claim is not available.");
        }
        
        Elder? elder = await elderRepository.Query().FirstOrDefaultAsync(m => m.Email == userClaim.Value);
        if (elder == null)
        {
            _logger.LogError("Elder not found.");
            return NotFound();
        }

        if (elder.MacAddress == null)
        {
            _logger.LogError("Arduino address is null.");
            return BadRequest("Arduino address is already null.");
        }
        _logger.LogInformation("Removing Arduino address for elder {elder.Email}.", elder.Email);
        elder.MacAddress = null;
        try
        {
            await elderRepository.Update(elder);
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
        IRepository<Elder> elderRepository = _repositoryFactory.GetRepository<Elder>();
        
        Elder? elder = await elderRepository.Query().FirstOrDefaultAsync(m => m.Email == elderEmail);
        if (elder == null)
        {
            _logger.LogError("Elder not found.");
            return NotFound("Elder not found.");
        }
        if (string.IsNullOrEmpty(elder.MacAddress))
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
        IRepository<Elder> elderRepository = _repositoryFactory.GetRepository<Elder>();
        
        Elder? elder = await elderRepository.Query().FirstOrDefaultAsync(m => m.Email == elderEmail);
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
            await elderRepository.Update(elder);
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
    public async Task<ActionResult<List<GetElderDTO>>> GetInvites()
    {
        IRepository<Caregiver> caregiverRepository = _repositoryFactory.GetRepository<Caregiver>();
        
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
        {
            _logger.LogError("User claim is null or empty.");
            return BadRequest("User claim is not available.");
        }

        // Include Elders when retrieving the Caregiver
        Caregiver? caregiver = await caregiverRepository.Query()
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
            List<GetElderDTO> invites = new List<GetElderDTO>();
            foreach (Elder elder in caregiver.Invites)
            {
                invites.Add(new GetElderDTO
                {
                    Name = elder.Name,
                    Email = elder.Email,
                    role = Roles.Elder
                });
            }
            return invites;
        }
        _logger.LogError("Caregiver has no invites.");
        return BadRequest("Caregiver has no invites.");
    }

[HttpPost("caregiver/invites/accept")]
[Authorize(Roles = "Caregiver")]
public async Task<ActionResult> AcceptInvite(string elderEmail)
{
    IRepository<Caregiver> caregiverRepository = _repositoryFactory.GetRepository<Caregiver>();

    Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
    {
        _logger.LogError("User claim is null or empty.");
        return BadRequest("User claim is not available.");
    }

    // Include Invites and Elders collections
    Caregiver? caregiver = await caregiverRepository.Query()
        .Include(c => c.Invites)
        .Include(c => c.Elders)
        .FirstOrDefaultAsync(c => c.Email == userClaim.Value);
    if (caregiver == null)
    {
        _logger.LogError("Caregiver not found.");
        return BadRequest("Caregiver not found.");
    }

    if (caregiver.Invites == null || caregiver.Invites.Count == 0)
    {
        _logger.LogError("No invites found.");
        return BadRequest("No invites found.");
    }

    Elder? elder = caregiver.Invites.FirstOrDefault(m => m.Email == elderEmail);
    if (elder == null)
    {
        _logger.LogError("Elder not found.");
        return NotFound("Elder not found.");
    }

    _logger.LogInformation("Invites found. {invites}", caregiver.Invites.Select(i => i.Email));
    _logger.LogInformation("Elders found. {elders}", caregiver.Elders?.Select(e => e.Email));
    _logger.LogInformation("Elder found. {elder}", elder.Email);
    
    elder.CaregiverId = caregiver.Id;
    elder.InvitedCaregiverId = null;
    _dbContext.Entry(elder).State = EntityState.Modified;
    _dbContext.Update(elder);
    
    try
    {
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
    IRepository<Elder> elderRepository = _repositoryFactory.GetRepository<Elder>();
        
    Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
    if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
    {
        _logger.LogError("User claim is null or empty.");
        return BadRequest("User claim is not available.");
    }
        
    Elder? elder = await elderRepository.Query().FirstOrDefaultAsync(m => m.Email == userClaim.Value);
    if (elder == null)
    {
        _logger.LogError("Elder not found.");
        return NotFound();
    }

    if (elder.MacAddress != null) return elder.MacAddress;
    _logger.LogError("No mac address found.");
    return NotFound("No mac address found.");
}

    [HttpGet("users/elder/caregiver")]
    [Authorize(Roles = "Elder")]
    public async Task<ActionResult<List<CaregiverDTO>>> GetElderCaregiver()
    {
        IRepository<Elder> elderRepository = _repositoryFactory.GetRepository<Elder>();
        
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
        {
            _logger.LogError("User claim is null or empty.");
            return BadRequest("User claim is not available.");
        }
        
        Elder? elder = await elderRepository.Query().FirstOrDefaultAsync(m => m.Email == userClaim.Value);
        if (elder == null)
        {
            _logger.LogError("Elder not found.");
            return NotFound();
        }
        if (elder.CaregiverId == null)
        {
            _logger.LogError("No caregiver found.");
            return NotFound("No caregiver found.");
        }
        IRepository<Caregiver> caregiverRepository = _repositoryFactory.GetRepository<Caregiver>();
        
        Caregiver? caregiver = await caregiverRepository.Query().FirstOrDefaultAsync(m => m.Id == elder.CaregiverId);
        if (caregiver == null)
        {
            _logger.LogError("No caregiver found.");
            return NotFound("No caregiver found.");
        }
        
        List<CaregiverDTO> caregivers = new List<CaregiverDTO>
        {
            new CaregiverDTO
            {
                Name = caregiver.Name,
                Email = caregiver.Email
            }
        };
        
        return caregivers;
    }
    
    [HttpGet("renew/token")]
    [Authorize(AuthenticationSchemes = "ExpiredTokenScheme")]
    public async Task<ActionResult<string>> RenewToken()
    {
        IRepository<Elder> elderRepository = _repositoryFactory.GetRepository<Elder>();
        IRepository<Caregiver> caregiverRepository = _repositoryFactory.GetRepository<Caregiver>();
        
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
        {
            _logger.LogError("User claim is null or empty.");
            return BadRequest("User claim is not available.");
        }
        
        //Check if the user is an elder or a caregiver
        var elder = await elderRepository.Query().FirstOrDefaultAsync(m => m.Email == userClaim.Value);
        var caregiver = await caregiverRepository.Query().FirstOrDefaultAsync(m => m.Email == userClaim.Value);
        
        //Get time to expire from the token that the request maker has
        var expiredClaim = User.Claims.FirstOrDefault(c => c.Type == "exp");
        
        if (expiredClaim == null)
        {
            _logger.LogError("Expiration claim not found.");
            return BadRequest("Expiration claim not found.");
        }
        
        DateTime expTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiredClaim.Value)).DateTime;
        if (expTime > DateTime.UtcNow)
        {
            _logger.LogError("Token is not expired yet.");
            return BadRequest("Token is not expired yet.");
        }

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
