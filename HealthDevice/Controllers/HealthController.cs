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
        
        public HealthController(UserManager<Elder> elderManager, HealthService healthService, ILogger<HealthController> logger, GeoService geoService, ApplicationDbContext db)
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

            // Fetch historical heart rate data
            ActionResult<List<Heartrate>> data = await _healthService.GetHealthData<Heartrate>(
                elderEmail, periodEnum, date, e => true);

            if (data.Result is not BadRequestResult && data.Value != null && data.Value.Any())
            {
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
            }

            // Fetch current heart rate data if historical data is unavailable
            ActionResult<List<currentHeartRate>> currentHeartRateData = await _healthService.GetCurrentHealthData<currentHeartRate>(
                elderEmail, periodEnum, date,
                m => new currentHeartRate
                {
                    Heartrate = m.Heartrate,
                    Timestamp = m.Timestamp
                });

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

            // Calculate aggregated heart rate data for longer periods
            Heartrate heartrates = await _healthService.CalculateHeartRateFromUnproccessed(currentHeartRateData.Value);
            return new List<PostHeartRate>
            {
                new()
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

        [HttpGet("Spo2")]
        public async Task<ActionResult<List<PostSpo2>>> GetSpo2(string elderEmail, DateTime date, string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(typeof(Period), periodEnum))
            {
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }

            // Fetch historical SpO2 data
            ActionResult<List<Max30102>> data = await _healthService.GetHealthData<Max30102>(
                elderEmail, periodEnum, date, e => true);

            if (data.Result is not BadRequestResult && data.Value != null && data.Value.Any())
            {
                return data.Value.Select(spo2 => new PostSpo2
                {
                    Spo2 = new Spo2
                    {
                        SpO2 = spo2.SpO2,
                        Timestamp = spo2.Timestamp
                    }
                }).ToList();
            }

            // Fetch current SpO2 data if historical data is unavailable
            ActionResult<List<currentSpo2>> currentSpo2Data = await _healthService.GetCurrentHealthData<currentSpo2>(
                elderEmail, periodEnum, date,
                m => new currentSpo2
                {
                    SpO2 = m.SpO2,
                    Timestamp = m.Timestamp
                });

            if (currentSpo2Data.Result is BadRequestResult || currentSpo2Data.Value == null || !currentSpo2Data.Value.Any())
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

            // Calculate aggregated SpO2 data for longer periods
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

        [HttpGet("Distance")]
        public async Task<ActionResult<List<Kilometer>>> GetDistance(string elderEmail, DateTime date, string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(typeof(Period), periodEnum))
            {
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }

            // Fetch historical distance data
            ActionResult<List<Kilometer>> data = await _healthService.GetHealthData<Kilometer>(
                elderEmail, periodEnum, date, e => true);

            if (data.Result is not BadRequestResult && data.Value != null && data.Value.Any())
            {
                return data.Value;
            }

            return BadRequest("No distance data available for the specified parameters.");
        }
        
        [HttpGet("Steps")]
        public async Task<ActionResult<List<Steps>>> GetSteps(string elderEmail, DateTime date, string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(typeof(Period), periodEnum))
            {
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }

            // Fetch historical steps data
            ActionResult<List<Steps>> data = await _healthService.GetHealthData<Steps>(
                elderEmail, periodEnum, date, e => true);

            if (data.Result is not BadRequestResult && data.Value != null && data.Value.Any())
            {
                return data.Value;
            }

            return BadRequest("No steps data available for the specified parameters.");
        }

        [HttpGet("Dashboard")]
        public async Task<ActionResult<DashBoard>> GetDashBoardInfo(string elderEmail)
        {
            // Fetch the elder by email
            Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
            if (elder is null || string.IsNullOrEmpty(elder.Arduino))
            {
                _logger.LogError("Elder not found or Arduino not set for email: {ElderEmail}", elderEmail);
                return BadRequest("Elder not found or Arduino not set.");
            }

            string macAddress = elder.Arduino;

            // Query data objects using the MacAddress
            Max30102? max30102 = _db.MAX30102Data
                .Where(m => m.Address == macAddress)
                .OrderByDescending(m => m.Timestamp)
                .FirstOrDefault();

            Kilometer? kilometer = _db.Distance
                .Where(d => d.MacAddress == macAddress && d.Timestamp.Date == DateTime.UtcNow.Date)
                .OrderByDescending(d => d.Timestamp)
                .FirstOrDefault();

            Steps? steps = _db.Steps
                .Where(s => s.MacAddress == macAddress && s.Timestamp.Date == DateTime.UtcNow.Date)
                .OrderByDescending(s => s.Timestamp)
                .FirstOrDefault();

            Location? location = _db.GPSData
                .Where(g => g.Address == macAddress)
                .OrderByDescending(g => g.Timestamp)
                .Select(g => new Location
                {
                    Latitude = g.Latitude,
                    Longitude = g.Longitude,
                    Timestamp = g.Timestamp
                })
                .FirstOrDefault();

            if (location is null)
            {
                _logger.LogError("Location data not found for MacAddress: {MacAddress}", macAddress);
                return BadRequest("Location data not found.");
            }

            string address = await _geoService.GetAddressFromCoordinates(location.Latitude, location.Longitude);
            
            return new DashBoard
            {
                allFall = _db.FallInfo.Count(f => f.MacAddress == macAddress),
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

            // Fetch historical fall data
            ActionResult<List<FallInfo>> data = await _healthService.GetHealthData<FallInfo>(
                elderEmail, periodEnum, date, e => true);

            if (data.Result is not BadRequestResult && data.Value != null && data.Value.Any())
            {
                return data.Value;
            }

            return BadRequest("No fall data available for the specified parameters.");
        }

        [HttpGet("Coordinates")]
        public async Task<ActionResult<Location>> GetLocaiton(string elderEmail)
        {
            Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
            if (elder is null)
            {
                return BadRequest();
            }

            Location? location = _db.Location.FirstOrDefault(m => m.MacAddress == elder.Arduino);
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
            Location? location = _db.Location.FirstOrDefault(m => m.MacAddress == elder.Arduino);
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
            _db.Perimeter.Add(perimeter);
            try
            {
                await _db.SaveChangesAsync();
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