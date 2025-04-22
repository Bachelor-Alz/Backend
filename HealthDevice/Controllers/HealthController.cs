using System.Security.Claims;
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
        private readonly UserManager<Caregiver> _caregiverManager;
        
        public HealthController(UserManager<Elder> elderManager, HealthService healthService, ILogger<HealthController> logger, GeoService geoService, ApplicationDbContext db, UserManager<Caregiver> caregiverManager)
        {
            _elderManager = elderManager;
            _healthService = healthService;
            _logger = logger;
            _geoService = geoService;
            _db = db;
            _caregiverManager = caregiverManager;
        }


        [HttpGet("Heartrate")]
        public async Task<ActionResult<List<PostHeartRate>>> GetHeartrate(string elderEmail, DateTime date,
            string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(typeof(Period), periodEnum))
            {
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }
            // Fetch historical heart rate data
            List<Heartrate> data = await _healthService.GetHealthData<Heartrate>(
                elderEmail, periodEnum, date.ToUniversalTime(), e => true);
            
            // Fetch current heart rate data if historical data is unavailable
            List<Max30102> currentHeartRateData =
                await _healthService.GetHealthData<Max30102>(elderEmail, periodEnum, date.ToUniversalTime(), e => true);


            if (data.Count != 0)
            {
                return BadRequest("No data available for the specified parameters.");
            }

            var newestHr = currentHeartRateData.OrderByDescending(h => h.Timestamp).First();

            if (data.Count != 0)
            {
                return data.Select(hr =>
                    new PostHeartRate
                    {
                        CurrentHeartRate = new currentHeartRate
                        {
                            Heartrate = newestHr.Heartrate,
                            Timestamp = hr.Timestamp
                        },
                        Heartrate = new Heartrate
                        {
                            Avgrate = hr.Avgrate,
                            Maxrate = hr.Maxrate,
                            Minrate = hr.Minrate,
                            Timestamp = hr.Timestamp
                        }
                    }).ToList();
            }
            
            List<Heartrate> proccessHeartrates = new List<Heartrate>();
            if (periodEnum == Period.Hour)
            {
                Heartrate heartrate = new Heartrate
                {
                    Avgrate = (int)currentHeartRateData.Average(h => h.Heartrate),
                    Maxrate = currentHeartRateData.Max(h => h.Heartrate),
                    Minrate = currentHeartRateData.Min(h => h.Heartrate),
                    Timestamp = currentHeartRateData.First().Timestamp
                };
                return currentHeartRateData.Select(hr => new PostHeartRate
                {
                    CurrentHeartRate = new currentHeartRate
                    {
                        Heartrate = hr.Heartrate,
                        Timestamp = hr.Timestamp
                    },
                    Heartrate = heartrate
                }).ToList();
            }
            if(periodEnum == Period.Day)
            {
                List<Heartrate> hourlyData = currentHeartRateData
                    .GroupBy(h => h.Timestamp.Hour)
                    .Select(g => new Heartrate
                    {
                        Avgrate = (int)g.Average(h => h.Heartrate),
                        Maxrate = g.Max(h => h.Heartrate),
                        Minrate = g.Min(h => h.Heartrate),
                        Timestamp = g.First().Timestamp.Date.AddHours(g.Key)
                    }).ToList();

                proccessHeartrates.AddRange(hourlyData);
            }
            if(periodEnum == Period.Week)
            {
                List<Heartrate> dailyData = currentHeartRateData
                    .GroupBy(h => h.Timestamp.Date)
                    .Select(g => new Heartrate
                    {
                        Avgrate = (int)g.Average(h => h.Heartrate),
                        Maxrate = g.Max(h => h.Heartrate),
                        Minrate = g.Min(h => h.Heartrate),
                        Timestamp = g.Key
                    }).ToList();

                proccessHeartrates.AddRange(dailyData);
            }
            _logger.LogInformation("ProcessedData {Count}", proccessHeartrates.Count);
            if (proccessHeartrates.Count == 0)
            {
                return BadRequest("No data available for the specified parameters.");
            }
            return proccessHeartrates.Select(hr =>
                new PostHeartRate
                {
                    CurrentHeartRate = new currentHeartRate
                    {
                        Heartrate = newestHr.Heartrate,
                        Timestamp = hr.Timestamp
                    },
                    Heartrate = new Heartrate
                    {
                        Avgrate = hr.Avgrate,
                        Maxrate = hr.Maxrate,
                        Minrate = hr.Minrate,
                        Timestamp = hr.Timestamp
                    }
                }).ToList();
        }

        [HttpGet("Spo2")]
        public async Task<ActionResult<List<PostSpo2>>> GetSpo2(string elderEmail, DateTime date, string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(typeof(Period), periodEnum))
            {
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }

            // Fetch historical SpO2 data
            List<Spo2> data = await _healthService.GetHealthData<Spo2>(
                elderEmail, periodEnum, date.ToUniversalTime(), e => true);

            // Fetch current SpO2 data if historical data is unavailable
            List<Max30102> currentSpo2Data =
                await _healthService.GetHealthData<Max30102>(elderEmail, periodEnum, date.ToUniversalTime(), e => true);

            if (data.Count != 0)
            {
                return data.Select(spo2 =>
                    new PostSpo2
                    {
                        CurrentSpo2 = new currentSpo2
                        {
                            SpO2 = currentSpo2Data.OrderByDescending(s => s.Timestamp).FirstOrDefault()?.SpO2 ?? 0,
                            Timestamp = spo2.Timestamp
                        },
                        Spo2 = new Spo2
                        {
                            SpO2 = spo2.SpO2,
                            MaxSpO2 = spo2.MaxSpO2,
                            MinSpO2 = spo2.MinSpO2,
                            Timestamp = spo2.Timestamp
                        }
                    }).ToList();
            }

            List<Spo2> processedSpo2 = new List<Spo2>();
            if (periodEnum == Period.Hour)
            {
                Spo2 spo2 = new Spo2
                {
                    SpO2 = currentSpo2Data.Average(s => s.SpO2),
                    MaxSpO2 = currentSpo2Data.Max(s => s.SpO2),
                    MinSpO2 = currentSpo2Data.Min(s => s.SpO2),
                    Timestamp = currentSpo2Data.First().Timestamp
                };
                return currentSpo2Data.Select(s => new PostSpo2()
                {
                     CurrentSpo2= new currentSpo2
                    {
                        SpO2 = s.Heartrate,
                        Timestamp = s.Timestamp
                    },
                    Spo2 = spo2
                }).ToList();
            }
            if (periodEnum == Period.Day)
            {
                List<Spo2> hourlyData = currentSpo2Data
                    .GroupBy(s => s.Timestamp.Hour)
                    .Select(g => new Spo2
                    {
                        SpO2 = g.Average(s => s.SpO2),
                        MaxSpO2 = g.Max(s => s.SpO2),
                        MinSpO2 = g.Min(s => s.SpO2),
                        Timestamp = g.First().Timestamp.Date.AddHours(g.Key)
                    }).ToList();

                processedSpo2.AddRange(hourlyData);
            }
            if (periodEnum == Period.Week)
            {
                List<Spo2> dailyData = currentSpo2Data
                    .GroupBy(s => s.Timestamp.Date)
                    .Select(g => new Spo2
                    {
                        SpO2 = g.Average(s => s.SpO2),
                        MaxSpO2 = g.Max(s => s.SpO2),
                        MinSpO2 = g.Min(s => s.SpO2),
                        Timestamp = g.Key
                    }).ToList();

                processedSpo2.AddRange(dailyData);
            }
            _logger.LogInformation("ProcessedData {Count}", processedSpo2.Count);
            if (processedSpo2.Count == 0)
            {
                return BadRequest("No data available for the specified parameters.");
            }
            return processedSpo2.Select(spo2 =>
                new PostSpo2
                {
                    CurrentSpo2 = new currentSpo2
                    {
                        SpO2 = currentSpo2Data.OrderByDescending(s => s.Timestamp).FirstOrDefault()?.SpO2 ?? 0,
                        Timestamp = spo2.Timestamp
                    },
                    Spo2 = new Spo2
                    {
                        SpO2 = spo2.SpO2,
                        MaxSpO2 = spo2.MaxSpO2,
                        MinSpO2 = spo2.MinSpO2,
                        Timestamp = spo2.Timestamp
                    }
                }).ToList();
        }

        [HttpGet("Distance")]
        public async Task<ActionResult<List<Kilometer>>> GetDistance(string elderEmail, DateTime date, string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(typeof(Period), periodEnum))
            {
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }

            // Fetch historical distance data
            List<Kilometer> data = await _healthService.GetHealthData<Kilometer>(
                elderEmail, periodEnum, date.ToUniversalTime(), e => true);

            if (data.Count != 0)
            {
                return data;
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
            List<Steps> data = await _healthService.GetHealthData<Steps>(
                elderEmail, periodEnum, date.ToUniversalTime(), e => true);

            _logger.LogInformation("Steps data count: {Count}", data.Count);
            
            if (data.Count != 0)
            {
                return data;
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
                .Where(d => d.MacAddress == macAddress)
                .OrderByDescending(d => d.Timestamp)
                .FirstOrDefault();

            Steps? steps = _db.Steps
                .Where(s => s.MacAddress == macAddress)
                .OrderByDescending(s => s.Timestamp)
                .FirstOrDefault();

            Location? location = _db.Location
                .Where(m => m.MacAddress == macAddress)
                .OrderByDescending(m => m.Timestamp)
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
            List<FallInfo> data = await _healthService.GetHealthData<FallInfo>(
                elderEmail, periodEnum, date.ToUniversalTime(), e => true);

            if (data.Count != 0)
            {
                return data;
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

        [HttpGet("Coordinates/Elders")]
        [Authorize(Roles = "Caregiver")]
        public async Task<ActionResult<List<ElderLocation>>> GetEldersLocation()
        {
            Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            {
                _logger.LogError("User claim is null or empty.");
                return BadRequest("User claim is not available.");
            }

            Caregiver? caregiver = await _caregiverManager.FindByEmailAsync(userClaim.Value);
            if (caregiver == null)
            {
                _logger.LogError("Caregiver not found.");
                return BadRequest("Caregiver not found.");
            }

            List<Elder>? elders = caregiver.Elders;
            if (elders != null)
            {
                List<ElderLocation> elderLocations = new List<ElderLocation>();
                foreach (Elder elder in elders)
                {
                    Location? location = _db.Location.FirstOrDefault(m => m.MacAddress == elder.Arduino);
                    if (location != null)
                    {
                        if (elder.Email != null)
                            elderLocations.Add(new ElderLocation
                            {
                                email = elder.Email,
                                name = elder.Name,
                                latitude = location.Latitude,
                                longitude = location.Longitude
                            });
                    }
                }
                return elderLocations;
            }
            else
            {
                _logger.LogError("No elders found for the caregiver.");
                return BadRequest("No elders found for the caregiver.");
            }
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