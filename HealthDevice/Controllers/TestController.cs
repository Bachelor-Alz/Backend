using HealthDevice.Models;
using HealthDevice.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
// ReSharper disable SuggestVarOrType_SimpleTypes

namespace HealthDevice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly IRepository<Elder> _elderRepository;
    private readonly IRepository<Max30102> _max30102Repository;
    private readonly IRepository<DistanceInfo> _kilometerRepository;
    private readonly IRepository<Steps> _stepsRepository;
    private readonly IRepository<FallInfo> _fallInfoRepository;
    private readonly IRepository<Location> _locationRepository;
    private readonly IRepository<GPSData> _gpsRepository;

    public TestController
    (
        IRepository<Elder> elderRepository,
        IRepository<Max30102> max30102Repository,
        IRepository<DistanceInfo> kilometerRepository,
        IRepository<Steps> stepsRepository,
        IRepository<FallInfo> fallInfoRepository,
        IRepository<Location> locationRepository,
        IRepository<GPSData> gpsRepository
    )
    {
        _elderRepository = elderRepository;
        _max30102Repository = max30102Repository;
        _kilometerRepository = kilometerRepository;
        _stepsRepository = stepsRepository;
        _fallInfoRepository = fallInfoRepository;
        _locationRepository = locationRepository;
        _gpsRepository = gpsRepository;
    }
    
    [HttpPost("FakeData")]
    public async Task<ActionResult> GenerateFakeData(string elderEmail)
    {
        Elder? elder = await _elderRepository.Query()
            .FirstOrDefaultAsync(e => e.Email == elderEmail);
        if (elder == null || string.IsNullOrEmpty(elder.MacAddress))
        {
            return NotFound("Elder not found");
        }
        
        DateTime currentDate = DateTime.UtcNow.Date;
        const double spo2Min = 0.7;
        const double spo2Max = 1.0;
        const int stepsMin = 0;
        const int stepsMax = 328; //Corresponds to 0.25 km
        const float distanceMin = 0;
        const float distanceMax = 0.25F; //Corresponds to 328 Steps
        const int fallMin = 0;
        const int fallMax = 20;

        for (int i = -1500; i < 1500; i++)
        {
            DateTime timestamp = currentDate + TimeSpan.FromMinutes(i * 5);
            int preHeartrateMin = Random.Shared.Next(20, 45);
            int preHeartrateMax = Random.Shared.Next(140, 200);
            int heartrate = Random.Shared.Next(preHeartrateMin, preHeartrateMax);
            int minheartrate = Random.Shared.Next(preHeartrateMin, heartrate);
            int maxheartrate = Random.Shared.Next(heartrate, preHeartrateMax);
            float spo2 = Convert.ToSingle(Random.Shared.NextDouble() * (spo2Max - spo2Min) + spo2Min);
            float minSpo2 = Convert.ToSingle(Random.Shared.NextDouble() * (spo2 - spo2Max) + spo2);
            float maxSpo2 = Convert.ToSingle(Random.Shared.NextDouble() * (spo2Max - spo2) + spo2);

            await _max30102Repository.Add(new Max30102
            {
                LastHeartrate = heartrate,
                AvgHeartrate = heartrate,
                MaxHeartrate = maxheartrate,
                MinHeartrate = minheartrate,
                LastSpO2 = spo2,
                AvgSpO2 = spo2,
                MaxSpO2 = maxSpo2,
                MinSpO2 = minSpo2,
                Timestamp = timestamp,
                MacAddress = elder.MacAddress
            });

            int steps = Random.Shared.Next(stepsMin, stepsMax);
            float distance = (float)(Random.Shared.NextDouble() * (distanceMax - distanceMin) + distanceMin);

            await _stepsRepository.Add(new Steps
            {
                StepsCount = steps,
                Timestamp = timestamp,
                MacAddress = elder.MacAddress
            });
            await _kilometerRepository.Add(new DistanceInfo()
            {
                Distance = distance,
                Timestamp = timestamp,
                MacAddress = elder.MacAddress
            });
        }

        await _gpsRepository.Add(new GPSData()
        {
            Latitude = 57.012153,
            Longitude = 9.991292,
            Timestamp = currentDate,
            MacAddress = elder.MacAddress
        });

        await _locationRepository.Add(new Location
        {
            Latitude = 57.012153,
            Longitude = 9.991292,
            Timestamp = currentDate,
            MacAddress = elder.MacAddress
        });

        for (int k = 0; k < 100; k++)
        {
            DateTime timestamp = currentDate.Date + TimeSpan.FromDays(k);
            for (int j = 1; j <= 24; j++)
            {
                int fall = Random.Shared.Next(fallMin, fallMax);
                DateTime timestamp2 = timestamp + TimeSpan.FromHours(j);
                if (fall == 7)
                {
                    await _fallInfoRepository.Add(new FallInfo
                    {
                        Location = new Location
                        {
                            Latitude = 57.012153,
                            Longitude = 9.991292,
                            Timestamp = timestamp2,
                            MacAddress = elder.MacAddress
                        },
                        Timestamp = timestamp2,
                        MacAddress = elder.MacAddress
                    });
                }
            }
        }

        return Ok("Fake data generated successfully");
    }
}
