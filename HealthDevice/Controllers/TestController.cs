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
    public async Task<ActionResult> GetCoordinates(Address address)
    {
        var result = await _geoService.GetCoordinatesFromAddress(address.Street, address.City);
        return Ok(result);
    }
    [HttpPost("FakeData")]
    public async Task<ActionResult> GenerateFakeData(string elderEmail)
    {
        Elder? elder = await _elderManager.Users
            .FirstOrDefaultAsync(e => e.Email == elderEmail);
        if (elder == null)
        {
            return NotFound("Elder not found");
        }

        string? macAddress = elder.Arduino;
        
        if (string.IsNullOrEmpty(macAddress))
        {
            return BadRequest("Elder does not have a MAC address");
        }
        DateTime currentDate = DateTime.UtcNow;
        const int heartrateMin = 40;
        const int heartrateMax = 200;
        const double spo2Min = 0.7;
        const double spo2Max = 1.0;
        const int stepsMin = 0;
        const int stepsMax = 1000;
        const double distanceMin = 0.0;
        const double distanceMax = 10.0;
        const int fallMin = 0;
        const int fallMax = 10;

        for (int i = -15000; i < 15000; i++)
        {
            DateTime timestamp = currentDate + TimeSpan.FromMinutes(i*5);
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
                       
        for (int j = -15000; j < 15000; j++)
        {
            int steps = Random.Shared.Next(stepsMin, stepsMax);
            DateTime timestamp = currentDate.Date + TimeSpan.FromMinutes(j);
            double distance = Random.Shared.NextDouble() * (distanceMax - distanceMin) + distanceMin;
            
            _dbContext.Steps.Add(new Steps
            {
                StepsCount = steps,
                Timestamp = timestamp,
                MacAddress = macAddress
            });
            _dbContext.Distance.Add(new Kilometer
            {
                Distance = distance,
                Timestamp = currentDate,
                MacAddress = macAddress
            });
        }


        _dbContext.Location.Add(new Location
        {
            Latitude = 57.012153,
            Longitude = 9.991292,
            Timestamp = currentDate,
            MacAddress = macAddress
        });

        for(int k = 0; k < 100; k++)
        {
            DateTime timestamp = currentDate.Date + TimeSpan.FromDays(k);
            for (int j = 1; j <= 24; j++)
            {
                int fall = Random.Shared.Next(fallMin, fallMax);
                DateTime timestamp2 = timestamp + TimeSpan.FromHours(j);
                if (fall == 7)
                {
                    _dbContext.FallInfo.Add(new FallInfo
                    {
                        Location = new Location
                        {
                            Latitude = 57.012153,
                            Longitude = 9.991292,
                            Timestamp = timestamp2,
                            MacAddress = macAddress
                        },
                        Timestamp = timestamp2,
                        MacAddress = macAddress
                    });
                }
            }
        }
        
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
