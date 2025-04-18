using HealthDevice.Data;
using HealthDevice.DTO;
using HealthDevice.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthDevice.Controllers;

[Route("api/[controller]")]
[ApiController]
//This controller is used to test what ever endpoint we what to test if it works without implementing it in a main controller
public class TestController : ControllerBase
{
    private readonly GeoService _geoService;
    private readonly UserManager<Elder> _elderManager;
    private readonly ApplicationDbContext _dbContext;
    
    public TestController(GeoService geoService, UserManager<Elder> elderManager, ApplicationDbContext dbContext)
    {
        _geoService = geoService;
        _elderManager = elderManager;
        _dbContext = dbContext;
    }
    
    [HttpPost("Address")]
    public async Task<ActionResult> GetAddress(double latitude, double longitude)
    {
        var result = await _geoService.GetAddressFromCoordinates(latitude, longitude);
        return Ok(result);
    }
    
    [HttpPost("Coordinates")]
    public async Task<ActionResult> GetCoordinates(Address address, string? amenity = null)
    {
        var result = await _geoService.GetCoordinatesFromAddress(address.Street, address.City, address.State, address.Country, address.ZipCode, amenity);
        return Ok(result);
    }
    [HttpPost("FakeData")]
    public async Task<ActionResult> GenerateFakeData(string elderEmail)
    {
        Elder? elder = await _elderManager.Users.Include(e => e.Distance)
            .Include(e => e.Steps)
            .Include(e => e.FallInfo)
            .FirstOrDefaultAsync(e => e.Email == elderEmail);
        if (elder == null)
        {
            return NotFound("Elder not found");
        }

        string macAddress = elder.Arduino;
        
        if (string.IsNullOrEmpty(macAddress))
        {
            return BadRequest("Elder does not have a MAC address");
        }
        DateTime currentDate = DateTime.UtcNow;
        int heartrateMin = 40;
        int heartrateMax = 200;
        double spo2Min = 0.7;
        double spo2Max = 1.0;

        for (int i = 0; i < 30000; i++)
        {
            DateTime timestamp = currentDate.AddMinutes(i*5);
            int heartrate = Random.Shared.Next(heartrateMin, heartrateMax);
            float spo2 = Convert.ToSingle(Random.Shared.NextDouble() * (spo2Max - spo2Min) + spo2Min);

            _dbContext.MAX30102Data.Add(new Max30102
            {
                Heartrate = heartrate,
                SpO2 = spo2,
                Timestamp = timestamp,
                Address = macAddress    
            });
        }

        _dbContext.GPSData.Add(new GPS
        {
            Latitude = 57.012153,
            Longitude = 9.991292,
            Timestamp = currentDate,
            Address = macAddress
        });
        
        elder.Distance.Add(new Kilometer
        {
            Distance = 2.7,
            Timestamp = currentDate
        });
        
        elder.Steps.Add(new Steps
        {
            StepsCount = 1000,
            Timestamp = currentDate
        });

        try
        {
            await _dbContext.SaveChangesAsync();
            await _elderManager.UpdateAsync(elder);
            return Ok("Fake data generated successfully");
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to generate fake data: {ex.Message}");
        }
    }
}
