using HealthDevice.Data;
using HealthDevice.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthDevice.Controllers;


[Authorize]
[Route("api/[controller]")]
[ApiController]
public class GPSController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Elder> _elderManager; 
    private readonly UserManager<Caregiver> _caregiverManager;
    private readonly ILogger<GPSController> _logger;
    
    public GPSController(ApplicationDbContext context, UserManager<Elder> elderManager, UserManager<Caregiver> caregiverManager,ILogger<GPSController> logger)
    {
        _context = context;
        _elderManager = elderManager;
        _caregiverManager = caregiverManager;
        _logger = logger;
    }
    
    [HttpGet("location/get")]
    public async Task<ActionResult<Location>> Location(ElderLocationDTO elderLocationDTO)
    {
        if(elderLocationDTO == null)
        {
            _logger.LogWarning("ElderLocationDTO is null.");
            return BadRequest();
        }
        Elder? elder = await _elderManager.FindByNameAsync(elderLocationDTO.name);
        if(elder == null)
        {
            _logger.LogWarning("Elder not found.");
            return NotFound();
        }
        
        Location? location = await _context.Locations.LastOrDefaultAsync(l => l.id == elder.location.id);
        if(location == null)
        {
            _logger.LogWarning("Location not found.");
            return NotFound();
        }
        
        return location;
    }
    
    [HttpPost("location/post")]
    public async Task<ActionResult> PostPerimeter(Location location, int radius, string elderMail)
    {
        if(location == null)
        {
            _logger.LogWarning("Location is null.");
            return BadRequest();
        }
        
        try
        {
            await _context.Locations.AddAsync(location);
            Elder? elder = await _elderManager.FindByEmailAsync(elderMail);
            if(elder == null)
            {
                _logger.LogWarning("Elder not found.");
                return NotFound();
            }
            elder.perimeter = new Perimiter
            {
                location = location,
                radius = radius,
            };
            await _context.SaveChangesAsync();
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to add location.");
            return BadRequest();
        }
        
    }
} 

