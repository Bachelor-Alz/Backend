using HealthDevice.DTO;
using HealthDevice.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthDevice.Controllers;

[Route("api/[controller]")]
[ApiController]
//This controller is used to test what ever endpoint we what to test if it works without implementing it in a main controller
public class TestController : ControllerBase
{
    private readonly IGeoService _geoService;
    private readonly IRepositoryFactory _repositoryFactory;
    
    public TestController(IGeoService geoService, IRepositoryFactory repositoryFactory)
    {
        _geoService = geoService;
        _repositoryFactory = repositoryFactory;
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
        IRepository<Elder> elderRepository = _repositoryFactory.GetRepository<Elder>();
        IRepository<Max30102> max30102Repository = _repositoryFactory.GetRepository<Max30102>();
        IRepository<GPS> gpsRepository = _repositoryFactory.GetRepository<GPS>();
        IRepository<Steps> stepsRepository = _repositoryFactory.GetRepository<Steps>();
        IRepository<Kilometer> kilometerRepository = _repositoryFactory.GetRepository<Kilometer>();
        IRepository<FallInfo> fallInfoRepository = _repositoryFactory.GetRepository<FallInfo>();
        IRepository<Location> locationRepository = _repositoryFactory.GetRepository<Location>();
        Elder? elder = await elderRepository.Query()
            .FirstOrDefaultAsync(e => e.Email == elderEmail);
        if (elder == null)
        {
            return NotFound("Elder not found");
        }

        string? macAddress = elder.MacAddress;
        
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

        for (int i = -1500; i < 1500; i++)
        {
            DateTime timestamp = currentDate + TimeSpan.FromMinutes(i*5);
            int heartrate = Random.Shared.Next(heartrateMin, heartrateMax);
            float spo2 = Convert.ToSingle(Random.Shared.NextDouble() * (spo2Max - spo2Min) + spo2Min);

            await max30102Repository.Add(new Max30102
            {
                Heartrate = heartrate,
                SpO2 = spo2,
                Timestamp = timestamp,
                MacAddress = macAddress
            });
        }

        await gpsRepository.Add(new GPS
        {
            Latitude = 57.012153,
            Longitude = 9.991292,
            Timestamp = currentDate,
            MacAddress = macAddress
        });
                       
        for (int j = -1500; j < 1500; j++)
        {
            int steps = Random.Shared.Next(stepsMin, stepsMax);
            DateTime timestamp = currentDate.Date + TimeSpan.FromMinutes(j*5);
            double distance = Random.Shared.NextDouble() * (distanceMax - distanceMin) + distanceMin;
            
            await stepsRepository.Add(new Steps
            {
                StepsCount = steps,
                Timestamp = timestamp,
                MacAddress = macAddress
            });
            await kilometerRepository.Add(new Kilometer
            {
                Distance = distance,
                Timestamp = timestamp,
                MacAddress = macAddress
            });
        }


        await locationRepository.Add(new Location
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
                    await fallInfoRepository.Add(new FallInfo
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

        return Ok("Fake data generated successfully");
    }
}
