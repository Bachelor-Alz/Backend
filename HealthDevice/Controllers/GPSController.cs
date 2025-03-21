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
    private readonly UserManager<User> _userManager; 
    private readonly ILogger<GPSController> _logger;
    
    public GPSController(ApplicationDbContext context, UserManager<User> userManager, ILogger<GPSController> logger)
    {
        _context = context;
        _userManager = userManager;
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
        Elder? elder = await _context.Elders.FirstOrDefaultAsync(e => e.id == elderLocationDTO.id);
        if(elder == null)
        {
            _logger.LogWarning("Elder not found.");
            return NotFound();
        }
        
        Location? location = await _context.Locations.LastOrDefaultAsync(l => l.id == elder.locations.id);
        if(location == null)
        {
            _logger.LogWarning("Location not found.");
            return NotFound();
        }
        
        return location;
    }
    
    [HttpPost("location/post")]
    public async Task<ActionResult> PostLocation(Location location, int elderId)
    {
        if(location == null)
        {
            _logger.LogWarning("Location is null.");
            return BadRequest();
        }
        
        try
        {
            await _context.Locations.AddAsync(location);
            Elder? elder = await _context.Elders.FirstOrDefaultAsync(e => e.id == elderId);
            if(elder == null)
            {
                _logger.LogWarning("Elder not found.");
                return NotFound();
            }
            elder.locations = location;
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