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
    private readonly IRepository<Elder> _elderRepository;
    private readonly IRepository<Caregiver> _caregiverRepository;
    private readonly ApplicationDbContext _dbContext;
    private readonly TokenService _tokenService;


    public UserController
    (
        UserManager<Elder> elderManager,
        UserManager<Caregiver> caregiverManager,
        IUserService userService,
        ILogger<UserController> logger,
        IRepository<Elder> elderRepository,
        IRepository<Caregiver> caregiverRepository,
        ApplicationDbContext dbContext,
        TokenService tokenService
    )
    {
        _elderManager = elderManager;
        _caregiverManager = caregiverManager;
        _userService = userService;
        _logger = logger;
        _elderRepository = elderRepository;
        _caregiverRepository = caregiverRepository;
        _dbContext = dbContext;
        _tokenService = tokenService;
    }


    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDTO>> Login(UserLoginDTO userLoginDto)
    {
        if (string.IsNullOrEmpty(userLoginDto.Email) || string.IsNullOrEmpty(userLoginDto.Password) ||
            !userLoginDto.Email.Contains('@') || userLoginDto.Password.Length < 6)
            return BadRequest("Email and password are in wrong format.");

        _logger.LogInformation("Login attempt for Email: {Email}", userLoginDto.Email);
        return await _userService.HandleLogin(userLoginDto,
            HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown");
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult> Register(UserRegisterDTO userRegisterDto)
    {
        _logger.LogInformation("Registration attempt for Email: {Email} with Role: {Role}", userRegisterDto.Email,
            userRegisterDto.Role);

        if (string.IsNullOrEmpty(userRegisterDto.Email) || string.IsNullOrEmpty(userRegisterDto.Password) ||
            !userRegisterDto.Email.Contains('@') || userRegisterDto.Password.Length < 6)
            return BadRequest("Email and password are in wrong format.");

        if (userRegisterDto.Role != Roles.Elder && userRegisterDto.Role != Roles.Caregiver)
            return BadRequest("Invalid Role.");

        if (userRegisterDto.Role == Roles.Elder &&
            (userRegisterDto.Latitude == null || userRegisterDto.Longitude == null))
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
                    Email = userRegisterDto.Email.ToLowerInvariant(),
                    UserName = userRegisterDto.Email.ToLowerInvariant(),
                    Elders = new List<Elder>()
                }, HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown");
    }

    [HttpPost("users/elder")]
    [Authorize(Roles = "Elder")]
    public async Task<ActionResult> InviteCareGiver(string caregiverEmail)
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            return BadRequest("User claim is not available.");

        Caregiver? caregiver = await _caregiverRepository.Query()
            .Include(c => c.Invites)
            .FirstOrDefaultAsync(c => c.Email == caregiverEmail);

        if (caregiver == null)
            return BadRequest("Caregiver not found.");

        Elder? elder = await _elderRepository.Query().FirstOrDefaultAsync(e => e.Id == userClaim.Value);
        if (elder == null)
            return NotFound("Elder not found.");


        if (caregiver.Invites != null && caregiver.Invites.Any(e => e.Id == elder.Id))
        {
            _logger.LogInformation("Elder {elder.Email} is already invited by Caregiver {caregiver.Name}.", elder.Email,
                caregiver.Name);
            return BadRequest("Elder is already invited by this caregiver.");
        }

        caregiver.Invites ??= [];
        caregiver.Invites.Add(elder);

        try
        {
            _dbContext.Update(caregiver);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Elder {elder.Email} sent an invite to Caregiver {caregiver.Name}.", elder.Email,
                caregiver.Name);
            return Ok("Caregiver invited successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to invite caregiver {caregiver} to elder {elder}.", caregiver.Name,
                elder.Name);
            return BadRequest("Failed to invite caregiver.");
        }
    }

    [HttpDelete("users/elder/removeCaregiver")]
    [Authorize(Roles = "Elder")]
    public async Task<ActionResult> RemoveCaregiver()
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            return BadRequest("User claim is not available.");

        Elder? elder = await _elderRepository.Query()
            .FirstOrDefaultAsync(e => e.Id == userClaim.Value);

        if (elder == null)
            return NotFound("Elder not found.");

        if (elder.CaregiverId == null)
            return BadRequest("No caregiver assigned to this elder.");

        try
        {
            elder.CaregiverId = null;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Caregiver removed from Elder {elder.Email}.", elder.Email);
            return Ok("Caregiver removed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove caregiver from elder {elder.Email}.", elder.Email);
            return BadRequest("Failed to remove caregiver.");
        }
    }

    [HttpDelete("users/caregiver/removeFromElder")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult> RemoveFromElder(string elderId)
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            return BadRequest("User claim is not available.");

        Caregiver? caregiver = await _caregiverRepository.Query()
            .Include(c => c.Elders)
            .FirstOrDefaultAsync(c => c.Id == userClaim.Value);

        if (caregiver == null || caregiver.Elders == null)
            return BadRequest("Caregiver not found.");

        Elder? elder = caregiver.Elders.FirstOrDefault(e => e.Email == elderId);

        if (elder == null)
            return NotFound("Elder not found.");

        try
        {
            elder.CaregiverId = null;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("{ElderEmail} removed from Caregiver {CaregiverEmail}.", elder.Email,
                caregiver.Email);
            return Ok("Elder removed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove elder {elder} from caregiver {caregiver}.", elder.Name,
                caregiver.Name);
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
            .FirstOrDefaultAsync(c => c.Id == userClaim.Value);
        if (caregiver?.Elders == null)
            return BadRequest("Caregiver not found or has no elders.");

        List<Elder> elders = caregiver.Elders;
        _logger.LogInformation("Caregiver {caregiver} has {Count} elders.", caregiver.Name, elders.Count);
        return elders.Select(e => new GetElderDTO
        {
            Name = e.Name,
            Email = e.Email,
            userId = e.Id,
            Role = Roles.Elder
        }).ToList();
    }

    [HttpGet("users/arduino")]
    public async Task<ActionResult<List<ArduinoInfoDTO>>> GetUnusedArduino()
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            return BadRequest("User claim is not available.");

        Elder? elder = await _elderRepository.Query().FirstOrDefaultAsync(m => m.Id == userClaim.Value);
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

        Elder? elder = await _elderRepository.Query().FirstOrDefaultAsync(m => m.Id == userClaim.Value);
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

        Elder? elder = await _elderRepository.Query().FirstOrDefaultAsync(m => m.Id == userClaim.Value);
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
    public async Task<ActionResult<bool>> IsConnected()
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            return BadRequest("User claim is not available.");

        Elder? elder = await _elderRepository.Query().FirstOrDefaultAsync(m => m.Id == userClaim.Value);
        if (!(elder == null || string.IsNullOrEmpty(elder.MacAddress))) return true;
        return NotFound("Elder not found.");
    }


    [HttpGet("caregiver/invites")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult<List<GetElderDTO>>> GetCaregiverInvites()
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            return BadRequest("User claim is not available.");

        Caregiver? caregiver = await _caregiverRepository.Query()
            .Include(c => c.Invites)
            .FirstOrDefaultAsync(c => c.Id == userClaim.Value);
        if (caregiver?.Invites == null)
        {
            return BadRequest("Caregiver has no invites.");
        }

        _logger.LogInformation("Caregiver has {Count} invites.", caregiver.Invites.Count);
        return caregiver.Invites.Select(elder => new GetElderDTO
            { Name = elder.Name, Email = elder.Email, userId = elder.Id, Role = Roles.Elder }).ToList();
    }

    [HttpPost("caregiver/invites/accept")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult> AcceptInvite(string elderId)
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            return BadRequest("User claim is not available.");

        Caregiver? caregiver = await _caregiverRepository.Query()
            .Include(c => c.Invites)
            .Include(c => c.Elders)
            .FirstOrDefaultAsync(c => c.Id == userClaim.Value);

        if (caregiver?.Invites == null || caregiver.Invites.Count == 0)
            return BadRequest("No invites found.");

        Elder? elder = caregiver.Invites.FirstOrDefault(m => m.Id == elderId);

        if (elder == null)
            return NotFound("Elder not found.");

        try
        {
            elder.CaregiverId = caregiver.Id;
            elder.InvitedCaregiverId = null;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Caregiver {caregiver.Name} accepted invite from Elder {elder.Email}.",
                caregiver.Name, elder.Email);
            return Ok("Invite accepted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update caregiver.");
            return BadRequest("Failed to update caregiver.");
        }
    }


    [HttpGet("users/elder/caregiver")]
    [Authorize(Roles = "Elder")]
    public async Task<ActionResult<List<CaregiverDTO>>> GetElderCaregiver()
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            return BadRequest("User claim is not available.");

        Elder? elder = await _elderRepository.Query()
            .Include(e => e.Caregiver)
            .FirstOrDefaultAsync(m => m.Id == userClaim.Value);

        if (elder?.Caregiver == null)
            return NotFound("Caregiver details are not available.");

        return new List<CaregiverDTO>
        {
            new()
            {
                Name = elder.Caregiver.Name,
                Email = elder.Caregiver.Email,
                UserId = elder.Caregiver.Id,
                Role = Roles.Caregiver
            }
        };
    }

    [HttpPost("revoke/token")]
    public async Task<ActionResult> RevokeToken(string token)
    {
        if (!await _tokenService.ValidateRefreshTokenAsync(token))
            return BadRequest("Invalid refresh token.");

        var ipAddress = HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
        await _tokenService.RevokeRefreshTokenAsync(token, ipAddress);
        return Ok("Token revoked successfully.");
    }

    [HttpPost("renew/token")]
    [Authorize(AuthenticationSchemes = "ExpiredTokenScheme")]
    public async Task<ActionResult<RefreshAndAccessTokenResult>> RenewToken(string token)
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            return BadRequest("User claim is not available.");

        var ipAddress = HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";

        _logger.LogInformation("Attempting token renewal for user {Email}", userClaim.Value);

        Elder? elder = await _elderRepository.Query()
            .Include(e => e.Caregiver)
            .FirstOrDefaultAsync(m => m.Id == userClaim.Value);

        if (elder == null)
        {
            Caregiver? caregiver = await _caregiverRepository.Query()
                .Include(c => c.Elders)
                .FirstOrDefaultAsync(m => m.Id == userClaim.Value);

            if (caregiver == null)
                return NotFound("User not found.");

            var refreshToken = await _tokenService.RotateRefreshTokenAsync(token, ipAddress);

            if (refreshToken == null)
                return BadRequest("Invalid refresh token.");

            var accessToken = _tokenService.GenerateAccessToken(caregiver, "Caregiver");
            return Ok(new RefreshAndAccessTokenResult { AccessToken = accessToken, RefreshToken = refreshToken.Token });
        }
        else
        {
            var refreshToken = await _tokenService.RotateRefreshTokenAsync(token, ipAddress);

            if (refreshToken == null)
                return BadRequest("Invalid refresh token.");

            var accessToken = _tokenService.GenerateAccessToken(elder, "Elder");
            return Ok(new RefreshAndAccessTokenResult { AccessToken = accessToken, RefreshToken = refreshToken.Token });
        }
    }
}