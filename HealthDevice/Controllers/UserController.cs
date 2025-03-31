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
public class UserController(
    UserManager<Elder> elderManager,
    UserManager<Caregiver> caregiverManager,
    UserService userService,
    ILogger<UserController> logger,
    ApplicationDbContext dbContext)
    : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDTO>> Login(UserLoginDTO userLoginDto)
    {
        return userLoginDto.Role == Roles.Elder 
            ? await userService.HandleLogin(elderManager, userLoginDto, "Elder", HttpContext) 
            : await userService.HandleLogin(caregiverManager, userLoginDto, "Caregiver", HttpContext);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult> Register(UserRegisterDTO userRegisterDto)
    {
        return userRegisterDto.Role == Roles.Elder 
            ? await userService.HandleRegister(elderManager, userRegisterDto, 
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
            : await userService.HandleRegister(caregiverManager, userRegisterDto, 
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
    public async Task<ActionResult<List<Elder>>> GetUsers() => await elderManager.Users.ToListAsync();


    [HttpPost("users/elder")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult> PutElder(string elderEmail)
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
        {
            logger.LogError("User claim is null or empty.");
            return BadRequest("User claim is not available.");
        }

        Caregiver? caregiver = await caregiverManager.FindByEmailAsync(userClaim.Value);
        if (caregiver == null)
        {
            logger.LogError("Caregiver not found.");
            return BadRequest("Caregiver not found.");
        }

        Elder? elder = await elderManager.FindByEmailAsync(elderEmail);
        if (elder == null)
        {
            logger.LogError("Elder not found.");
            return NotFound("Elder not found.");
        }

        caregiver.Elders.Add(elder);
        try
        {
            await caregiverManager.UpdateAsync(caregiver);
            logger.LogInformation("{elder.Email} added to Caregiver {caregiver.name}.", elder.Email, caregiver.Name);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update caregiver.");
            return BadRequest("Failed to update caregiver.");
        }
    }

    [HttpDelete("users/elder")]
    [Authorize(Roles = "Caregiver")]
    public async Task<ActionResult> RemoveElder(string elderEmail)
    {
        Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
        {
            logger.LogError("User claim is null or empty.");
            return BadRequest("User claim is not available.");
        }

        Caregiver? caregiver = await caregiverManager.FindByEmailAsync(userClaim.Value);
        if (caregiver == null)
        {
            logger.LogError("Caregiver not found.");
            return BadRequest("Caregiver not found.");
        }
        Elder? elder = await elderManager.FindByEmailAsync(elderEmail);
        if(elder == null)
        {
            logger.LogError("Elder not found.");
            return NotFound();
        }

        caregiver.Elders.Remove(elder);
        try
        {
            await caregiverManager.UpdateAsync(caregiver);
            logger.LogInformation("{elder.Email} removed from Caregiver {caregiver.name}.", elder.Email, caregiver.Name);
            return Ok();
        }
        catch
        {
            logger.LogError("Failed to update caregiver.");
            return BadRequest();
        }
    }
    
    [HttpGet("users/arduino")]
    public async Task<ActionResult<List<string?>>> GetUnusedArduino()
    {
        //Get a list of all Max30102 address that has not an elder associated with it
        List<string> address = await dbContext.MAX30102Data.Select(a => a.Address).Distinct().ToListAsync();
        List<string?> addressNotAssociated = address.Except(elderManager.Users.Select(e => e.Arduino)).ToList();
        
        return addressNotAssociated;
    }
    
    [HttpPost("users/arduino")]
    public async Task<ActionResult> SetArduino(string email, string address)
    {
        Elder? elder = await elderManager.FindByEmailAsync(email);
        if (elder == null)
        {
            logger.LogError("Elder not found.");
            return NotFound();
        }

        elder.MAX30102Data = await dbContext.MAX30102Data.Where(m => m.Address == address).ToListAsync();
        elder.GPSData = await dbContext.GPSData.Where(m => m.Address == address).ToListAsync();
        try
        {
            await elderManager.UpdateAsync(elder);
            logger.LogInformation("Arduino address set for {elder.Email}.", elder.Email);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update elder.");
            return BadRequest();
        }
    }
}
