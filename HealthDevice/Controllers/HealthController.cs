using HealthDevice.Data;
using HealthDevice.DTO;
using HealthDevice.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Period = HealthDevice.DTO.Period;

namespace HealthDevice.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HealthController : ControllerBase
    {
        private readonly UserManager<Elder> _elderManager;
        private readonly HealthService _healthService;
        private readonly ILogger<HealthController> _logger;
        private readonly GeoService _geoService;
        private readonly ApplicationDbContext _db;
        public HealthController(
            UserManager<Elder> elderManager,
            HealthService healthService,
            ILogger<HealthController> logger,
            GeoService geoService,
            ApplicationDbContext db)
        {
            _elderManager = elderManager;
            _healthService = healthService;
            _logger = logger;
            _geoService = geoService;
            _db = db;
        }
        
        
        [HttpGet("Heartrate")]
        public async Task<ActionResult<List<PostHeartRate>>> GetHeartrate(string elderEmail, DateTime date, string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(typeof(Period), periodEnum))
            {
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }

            ActionResult<List<Heartrate>> data = await _healthService.GetHealthData<Heartrate>(elderEmail, periodEnum, date, e => e.Heartrate, _elderManager);
            if (data.Result is not BadRequestResult && data.Value != null && data.Value.Any())
                return data.Value.Select(hr => new PostHeartRate
                {
                    Heartrate = new Heartrate
                    {
                        Avgrate = hr.Avgrate,
                        Maxrate = hr.Maxrate,
                        Minrate = hr.Minrate,
                        Timestamp = hr.Timestamp
                    }
                }).ToList();
            {
                ActionResult<List<currentHeartRate>> currentHeartRateData = await _healthService.GetCurrentHealthData<currentHeartRate>(
                    elderEmail, periodEnum, date,
                    m => new currentHeartRate
                    {
                        Heartrate = m.Heartrate,
                        Timestamp = m.Timestamp
                    },
                    _elderManager);

                if (currentHeartRateData.Result is BadRequestResult || currentHeartRateData.Value == null || !currentHeartRateData.Value.Any())
                {
                    return BadRequest("No data available for the specified parameters.");
                }

                if (periodEnum == Period.Hour)
                {
                    return currentHeartRateData.Value
                        .Select(hr => new PostHeartRate
                        {
                            CurrentHeartRate = hr
                        }).ToList();
                }
                Heartrate heartrates = await _healthService.CalculateHeartRateFromUnproccessed(currentHeartRateData.Value);
                return new List<PostHeartRate>
                {
                    new ()
                    {
                        Heartrate = new Heartrate
                        {
                            Avgrate = heartrates.Avgrate,
                            Maxrate = heartrates.Maxrate,
                            Minrate = heartrates.Minrate,
                            Timestamp = heartrates.Timestamp
                        }
                    }
                };
            }

        }

        [HttpGet("Spo2")]
        public async Task<ActionResult<List<PostSpo2>>> GetSpo2(string elderEmail, DateTime date,
            string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(typeof(Period), periodEnum))
            {
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }

            ActionResult<List<Spo2>> data =
                await _healthService.GetHealthData<Spo2>(elderEmail, periodEnum, date, e => e.SpO2, _elderManager);
            if (data.Result is not BadRequestResult && data.Value != null && data.Value.Any())
                return data.Value.Select(spo2 => new PostSpo2
                {
                    Spo2 = new Spo2
                    {
                        SpO2 = spo2.SpO2,
                        MaxSpO2 = spo2.MaxSpO2,
                        MinSpO2 = spo2.MinSpO2,
                        Timestamp = spo2.Timestamp
                    }
                }).ToList();
            {
                ActionResult<List<currentSpo2>> currentSpo2Data =
                    await _healthService.GetCurrentHealthData<currentSpo2>(
                        elderEmail, periodEnum, date,
                        m => new currentSpo2
                        {
                            SpO2 = m.SpO2,
                            Timestamp = m.Timestamp
                        },
                        _elderManager);

                if (currentSpo2Data.Result is BadRequestResult || currentSpo2Data.Value == null ||
                    !currentSpo2Data.Value.Any())
                {
                    return BadRequest("No data available for the specified parameters.");
                }

                if (periodEnum == Period.Hour)
                {
                    return currentSpo2Data.Value
                        .Select(spo2 => new PostSpo2
                        {
                            CurrentSpo2 = spo2
                        }).ToList();
                }

                Spo2 spo2Data = await _healthService.CalculateSpo2FromUnprocessed(currentSpo2Data.Value);
                return new List<PostSpo2>
                {
                    new()
                    {
                        Spo2 = new Spo2
                        {
                            SpO2 = spo2Data.SpO2,
                            MaxSpO2 = spo2Data.MaxSpO2,
                            MinSpO2 = spo2Data.MinSpO2,
                            Timestamp = spo2Data.Timestamp
                        }
                    }
                };
            }
        }

        [HttpGet("Distance")]
        public async Task<ActionResult<List<Kilometer>>> GetDistance(string elderEmail, DateTime date, string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(typeof(Period), periodEnum))
            {
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }
            return await _healthService.GetHealthData<Kilometer>(elderEmail, periodEnum, date, e => e.Distance, _elderManager);
        }
        
        [HttpGet("Steps")]
        public async Task<ActionResult<List<Steps>>> GetSteps(string elderEmail, DateTime date, string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(typeof(Period), periodEnum))
            {
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }
            return await _healthService.GetHealthData<Steps>(elderEmail, periodEnum, date, e => e.Steps, _elderManager);
        }

        [HttpGet("Dashboard")]
        public async Task<ActionResult<DashBoard>> GetDashBordInfo(string elderEmail)
        {
            Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
            if (elder is null)
            {
                return BadRequest();
            }
            Location? location = elder.Location ?? new()
            {
                Latitude = 0.1,
                Longitude = 0.1
            };
            string address = await _geoService.GetAddressFromCoordinates(location.Latitude, location.Longitude);
            Max30102? max30102 = _db.MAX30102Data.
                Where(c => c.Timestamp.Date == DateTime.UtcNow.Date && c.Address == elder.Arduino)
                .OrderByDescending(c => c.Timestamp)
                .FirstOrDefault();
            Kilometer? kilometer = elder.Distance?.FindLast(d => d.Timestamp.Date == DateTime.UtcNow.Date);
            Steps? steps = elder.Steps?.FindLast(d => d.Timestamp.Date == DateTime.UtcNow.Date);
            
            return new DashBoard
            {
                allFall = elder.FallInfo?.Count ?? 0,
                distance = kilometer?.Distance ?? 0,
                HeartRate = max30102?.Heartrate ?? 0,
                locationAdress = address,
                SpO2 = max30102?.SpO2 ?? 0,
                steps = steps?.StepsCount ?? 0
            };
        }
        
        [HttpGet("Falls")]
        public async Task<ActionResult<List<FallInfo>>> GetFalls(string elderEmail, DateTime date, string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(typeof(Period), periodEnum))
            {
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }
            return await _healthService.GetHealthData<FallInfo>(elderEmail, periodEnum, date, e => e.FallInfo, _elderManager);
        }

        [HttpGet("Coordinates")]
        public async Task<ActionResult<Location>> GetLocaiton(string elderEmail)
        {
            Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
            if (elder is null)
            {
                return BadRequest();
            }
            Location? location = elder.Location;
            if (location is null)
            {
                return BadRequest();
            }

            return location;
        }

        [HttpGet("Address")]
        public async Task<ActionResult<string>> GetAddress(string elderEmail)
        {
            Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
            if (elder is null)
            {
                return BadRequest();
            }
            Location? location = elder.Location;
            if (location is null)
            {
                return BadRequest();
            }
            string address = await _geoService.GetAddressFromCoordinates(location.Latitude, location.Longitude);
            if (string.IsNullOrEmpty(address))
            {
                return BadRequest();
            }

            return address;
        }

        [HttpPost("Perimeter")]
        public async Task<ActionResult> SetPerimeter(int radius, string elderEmail)
        {
            Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
            if (elder is null)
            {
                return BadRequest();
            }

            if (elder.latitude == null || elder.longitude == null)
            {
                _logger.LogError("No home address set");
                return BadRequest("No home address set");
            }

            Perimeter perimeter = new Perimeter
            {
                Latitude = elder.latitude,
                Longitude = elder.longitude,
                Radius = radius
            };
            elder.Perimeter = perimeter;
            try
            {
                await _elderManager.UpdateAsync(elder);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to set perimeter: {Error}", e.Message);
                return BadRequest("Failed to set perimeter");
            }
        }
    }
}