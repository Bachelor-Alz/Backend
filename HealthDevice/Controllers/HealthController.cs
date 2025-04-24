using System.Security.Claims;
using HealthDevice.Data;
using HealthDevice.DTO;
using HealthDevice.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly EmailService _emailService;
        
        public HealthController(UserManager<Elder> elderManager, HealthService healthService, ILogger<HealthController> logger, GeoService geoService, ApplicationDbContext db, UserManager<Caregiver> caregiverManager, EmailService emailService)
        {
            _elderManager = elderManager;
            _healthService = healthService;
            _logger = logger;
            _geoService = geoService;
            _db = db;
            _caregiverManager = caregiverManager;
            _emailService = emailService;
        }


        [HttpGet("Heartrate")]
        public async Task<ActionResult<List<PostHeartRate>>> GetHeartrate(string elderEmail, DateTime date,
            string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(typeof(Period), periodEnum))
            {
                _logger.LogError("Invalid period specified: {Period}", period);
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }
            // Fetch historical heart rate data
            List<Heartrate> data = await _healthService.GetHealthData<Heartrate>(
                elderEmail, periodEnum, date.ToUniversalTime(), e => true);
            _logger.LogInformation("Fetched historical heart rate data: {Count}", data.Count);
            // Fetch current heart rate data if historical data is unavailable
            List<Max30102> currentHeartRateData =
                await _healthService.GetHealthData<Max30102>(elderEmail, periodEnum, date.ToUniversalTime(), e => true);
            _logger.LogInformation("Fetched current heart rate data: {Count}", currentHeartRateData.Count);

            if (data.Count != 0)
            {
                _logger.LogInformation("Processing historical heart rate data for elder: {ElderEmail}", elderEmail);
                return BadRequest("No data available for the specified parameters.");
            }

            var newestHr = currentHeartRateData.OrderByDescending(h => h.Timestamp).First();

            if (data.Count != 0)
            {
                _logger.LogInformation("Processing historical heart rate data for elder: {ElderEmail}", elderEmail);
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
                _logger.LogInformation("Processing current heart rate data for elder: {ElderEmail}", elderEmail);
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
                    Heartrate = new Heartrate
                    {
                        Avgrate = heartrate.Avgrate,
                        Maxrate = heartrate.Maxrate,
                        Minrate = heartrate.Minrate,
                        Timestamp = hr.Timestamp
                    }
                }).ToList();
            }
            if(periodEnum == Period.Day)
            {
                _logger.LogInformation("Processing daily heart rate data for elder: {ElderEmail}", elderEmail);
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
                _logger.LogInformation("Processing weekly heart rate data for elder: {ElderEmail}", elderEmail);
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
                _logger.LogError("No processed heart rate data available for elder: {ElderEmail}", elderEmail);
                return BadRequest("No data available for the specified parameters.");
            }
            
            return proccessHeartrates.Count != 0
                ? proccessHeartrates.Select(hr =>
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
                    }).ToList()
                : [];
        }

        [HttpGet("Spo2")]
        public async Task<ActionResult<List<PostSpo2>>> GetSpo2(string elderEmail, DateTime date, string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(typeof(Period), periodEnum))
            {
                _logger.LogError("Invalid period specified: {Period}", period);
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }

            // Fetch historical SpO2 data
            List<Spo2> data = await _healthService.GetHealthData<Spo2>(
                elderEmail, periodEnum, date.ToUniversalTime(), e => true);
            _logger.LogInformation("Fetched historical SpO2 data: {Count}", data.Count);
            // Fetch current SpO2 data if historical data is unavailable
            List<Max30102> currentSpo2Data =
                await _healthService.GetHealthData<Max30102>(elderEmail, periodEnum, date.ToUniversalTime(), e => true);
            _logger.LogInformation("Fetched current SpO2 data: {Count}", currentSpo2Data.Count);
            if (data.Count != 0)
            {
                _logger.LogInformation("Processing historical SpO2 data for elder: {ElderEmail}", elderEmail);
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
                            AvgSpO2 = spo2.AvgSpO2,
                            MaxSpO2 = spo2.MaxSpO2,
                            MinSpO2 = spo2.MinSpO2,
                            Timestamp = spo2.Timestamp
                        }
                    }).ToList();
            }

            List<Spo2> processedSpo2 = new List<Spo2>();
            if (periodEnum == Period.Hour)
            {
                _logger.LogInformation("Processing current SpO2 data for elder: {ElderEmail}", elderEmail);
                Spo2 spo2 = new Spo2
                {
                    AvgSpO2 = currentSpo2Data.Average(s => s.SpO2),
                    MaxSpO2 = currentSpo2Data.Max(s => s.SpO2),
                    MinSpO2 = currentSpo2Data.Min(s => s.SpO2),
                    Timestamp = currentSpo2Data.First().Timestamp
                };
                return currentSpo2Data.Select(s => new PostSpo2
                {
                     CurrentSpo2= new currentSpo2
                    {
                        SpO2 = s.SpO2,
                        Timestamp = s.Timestamp
                    },
                    Spo2 = new Spo2
                    {
                        AvgSpO2 = spo2.AvgSpO2,
                        MaxSpO2 = spo2.MaxSpO2,
                        MinSpO2 = spo2.MinSpO2,
                        Timestamp = s.Timestamp
                    }
                }).ToList();
            }
            if (periodEnum == Period.Day)
            {
                _logger.LogInformation("Processing daily SpO2 data for elder: {ElderEmail}", elderEmail);
                List<Spo2> hourlyData = currentSpo2Data
                    .GroupBy(s => s.Timestamp.Hour)
                    .Select(g => new Spo2
                    {
                        AvgSpO2 = g.Average(s => s.SpO2),
                        MaxSpO2 = g.Max(s => s.SpO2),
                        MinSpO2 = g.Min(s => s.SpO2),
                        Timestamp = g.First().Timestamp.Date.AddHours(g.Key)
                    }).ToList();

                processedSpo2.AddRange(hourlyData);
            }
            if (periodEnum == Period.Week)
            {
                _logger.LogInformation("Processing weekly SpO2 data for elder: {ElderEmail}", elderEmail);
                List<Spo2> dailyData = currentSpo2Data
                    .GroupBy(s => s.Timestamp.Date)
                    .Select(g => new Spo2
                    {
                        AvgSpO2 = g.Average(s => s.SpO2),
                        MaxSpO2 = g.Max(s => s.SpO2),
                        MinSpO2 = g.Min(s => s.SpO2),
                        Timestamp = g.Key
                    }).ToList();

                processedSpo2.AddRange(dailyData);
            }
            _logger.LogInformation("ProcessedData {Count}", processedSpo2.Count);
            return processedSpo2.Count != 0
                ? processedSpo2.Select(spo2 =>
                    new PostSpo2
                    {
                        CurrentSpo2 = new currentSpo2
                        {
                            SpO2 = currentSpo2Data.OrderByDescending(s => s.Timestamp).FirstOrDefault()?.SpO2 ?? 0,
                            Timestamp = spo2.Timestamp
                        },
                        Spo2 = new Spo2
                        {
                            AvgSpO2 = spo2.AvgSpO2,
                            MaxSpO2 = spo2.MaxSpO2,
                            MinSpO2 = spo2.MinSpO2,
                            Timestamp = spo2.Timestamp
                        }
                    }).ToList()
                : [];
        }

        [HttpGet("Distance")]
        public async Task<ActionResult<List<Kilometer>>> GetDistance(string elderEmail, DateTime date, string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(typeof(Period), periodEnum))
            {
                _logger.LogError("Invalid period specified: {Period}", period);
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }

            if (periodEnum == Period.Hour)
            {
                _logger.LogInformation("Processing current distance data for elder: {ElderEmail}", elderEmail);
                DateTime newTime = new DateTime(date.Year, date.Month, date.Day, date.Hour+1, 0, 0).ToUniversalTime();
                List<Kilometer> data = await _healthService.GetHealthData<Kilometer>(
                    elderEmail, periodEnum, newTime, e => true);
                _logger.LogInformation("Fetched distance data: {Count}", data.Count);
                return data.Count != 0 ? data : [];

            }
            else
            {
                _logger.LogInformation("Processing daily distance data for elder: {ElderEmail}", elderEmail);
                DateTime newTime = new DateTime(date.Year, date.Month, date.Day+1, 0, 0, 0).ToUniversalTime();
                List<Kilometer> data = await _healthService.GetHealthData<Kilometer>(
                    elderEmail, periodEnum, newTime, e => true);
                _logger.LogInformation("Fetched distance data: {Count}", data.Count);
                return data.Count != 0 ? data : [];
            }
        }
        
        [HttpGet("Steps")]
        public async Task<ActionResult<List<Steps>>> GetSteps(string elderEmail, DateTime date, string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(typeof(Period), periodEnum))
            {
                _logger.LogError("Invalid period specified: {Period}", period);
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }

            if (Period.Hour == periodEnum)
            {
                _logger.LogInformation("Processing current steps data for elder: {ElderEmail}", elderEmail);
                DateTime newTime = new DateTime(date.Year, date.Month, date.Day, date.Hour+1, 0, 0).ToUniversalTime();
                List<Steps> data = await _healthService.GetHealthData<Steps>(
                    elderEmail, periodEnum, newTime, e => true);
                _logger.LogInformation("Fetched steps data: {Count}", data.Count);
                return data.Count != 0 ? data : [];
            }
            else
            {
                _logger.LogInformation("Processing daily steps data for elder: {ElderEmail}", elderEmail);
                DateTime newTime = new DateTime(date.Year, date.Month, date.Day+1, 0, 0, 0).ToUniversalTime();
                List<Steps> data = await _healthService.GetHealthData<Steps>(
                    elderEmail, periodEnum, newTime, e => true);
                _logger.LogInformation("Fetched steps data: {Count}", data.Count);
                return data.Count != 0 ? data : [];
            }
        }

        [HttpGet("Dashboard")]
        public async Task<ActionResult<DashBoard>> GetDashBoardInfo(string elderEmail)
        {
            // Fetch the elder by email
            Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
            if (elder is null || string.IsNullOrEmpty(elder.Arduino))
            {
                _logger.LogError("Elder not found or Arduino not set for email: {ElderEmail}", elderEmail);
                return new DashBoard();
            }
            _logger.LogInformation("Fetching dashboard data for elder: {ElderEmail}", elderEmail);

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

            string address = String.Empty;
            if(location != null)
            { 
                address = await _geoService.GetAddressFromCoordinates(location.Latitude, location.Longitude);
            } 
            
            _logger.LogInformation("Fetched data for elder: {ElderEmail}", elderEmail);
            
            return new DashBoard
            {
                allFall = _db.FallInfo.Count(f => f.MacAddress == macAddress),
                distance = kilometer?.Distance ?? 0,
                HeartRate = max30102?.Heartrate ?? 0,
                locationAdress = address ?? "No address found",
                SpO2 = max30102?.SpO2 ?? 0,
                steps = steps?.StepsCount ?? 0
            };
        }

        [HttpGet("Falls")]
        public async Task<ActionResult<List<FallDTO>>> GetFalls(string elderEmail, DateTime date,
            string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(typeof(Period), periodEnum))
            {
                _logger.LogError("Invalid period specified: {Period}", period);
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }

            if (periodEnum == Period.Hour)
            {
                _logger.LogInformation("Processing current fall data for elder: {ElderEmail}", elderEmail);
                DateTime newTime = new DateTime(date.Year, date.Month, date.Day, date.Hour + 1, 0, 0).ToUniversalTime();
                List<FallInfo> data = await _healthService.GetHealthData<FallInfo>(
                    elderEmail, periodEnum, newTime, e => true);
                _logger.LogInformation("Fetched fall data: {Count}", data.Count);
                List<FallDTO> result = new List<FallDTO>();
                foreach (var fall in data)
                {
                    result.Add(new FallDTO
                    {
                        Timestamp = fall.Timestamp,
                        fallCount = 1
                    });
                }
                _logger.LogInformation("Processed fall data: {Count}", result.Count);
                return result.Count != 0 ? result : [];
            }
            else
            {
                _logger.LogInformation("Processing daily fall data for elder: {ElderEmail}", elderEmail);
                DateTime newTime = new DateTime(date.Year, date.Month, date.Day + 1, 0, 0, 0).ToUniversalTime();
                List<FallInfo> data = await _healthService.GetHealthData<FallInfo>(
                    elderEmail, periodEnum, newTime, e => true);
                List<FallDTO> result = new List<FallDTO>();
                int i = 0;
                DateTime fallDate = data.First().Timestamp.Date;
                foreach (var fall in data)
                {
                    if (fall.Timestamp.Date == fallDate.Date)
                    {
                        i++;
                        result.Add(new FallDTO
                        {
                            Timestamp = fall.Timestamp,
                            fallCount = i
                        });
                    }
                    else
                    {
                        i = 0;
                        fallDate = fall.Timestamp.Date;
                        result.Add(new FallDTO
                        {
                            Timestamp = fall.Timestamp,
                            fallCount = i
                        });
                    }
                }
                _logger.LogInformation("Processed fall data: {Count}", result.Count);
                return result.Count != 0 ? result : [];
            }
        }
        

        [HttpGet("Coordinates")]
        public async Task<ActionResult<Location>> GetLocaiton(string elderEmail)
        {
            Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
            if (elder is null)
            {
                _logger.LogError("Elder not found for email: {ElderEmail}", elderEmail);
                return BadRequest("Elder not found.");
            }
            if (string.IsNullOrEmpty(elder.Arduino))
            {
                _logger.LogError("Elder Arduino not set for elder: {ElderEmail}", elderEmail);
                return BadRequest("Elder Arduino not set.");
            }
            _logger.LogInformation("Fetching location data for elder: {ElderEmail}", elderEmail);
            Location? location = _db.Location.FirstOrDefault(m => m.MacAddress == elder.Arduino);
            if (location is null)
            {
                _logger.LogError("Location not found for elder: {ElderEmail}", elderEmail);
                return BadRequest("Location not found.");
            }
            _logger.LogInformation("Fetched location data for elder: {ElderEmail}", elderEmail);
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
            _logger.LogInformation("Fetching elder locations for caregiver: {CaregiverEmail}", userClaim.Value);
            Caregiver? caregiver = await _caregiverManager.Users
                .Include(c => c.Elders)
                .FirstOrDefaultAsync(c => c.Email == userClaim.Value);
            if (caregiver == null)
            {
                _logger.LogError("Caregiver not found.");
                return BadRequest("Caregiver not found.");
            }
            List<Elder>? elders = caregiver.Elders;
            if (elders == null || elders.Count == 0)
            {
                _logger.LogError("No elders found for the caregiver.");
                return BadRequest("No elders found for the caregiver.");
            }
            _logger.LogInformation("Found {ElderCount} elders for caregiver: {CaregiverEmail}", elders.Count, userClaim.Value);
            List<ElderLocation> elderLocations = new List<ElderLocation>();
            foreach (Elder elder in elders)
            {
                if (string.IsNullOrEmpty(elder.Arduino))
                {
                    _logger.LogError("Elder Arduino not set for elder: {ElderEmail}", elder.Email);
                    continue;
                }
                _logger.LogInformation("Fetching location data for elder: {ElderEmail}", elder.Email);
                Location? location = _db.Location.FirstOrDefault(m => m.MacAddress == elder.Arduino);
                if (location != null)
                {
                    _logger.LogInformation("Fetched location data for elder: {ElderEmail}", elder.Email);
                    if (elder.Email != null)
                    {
                        _logger.LogInformation("Fetching perimeter data for elder: {ElderEmail}", elder.Email);
                        Perimeter? perimeter = _db.Perimeter.FirstOrDefault(m => m.MacAddress == elder.Arduino);
                        if (perimeter != null)
                        {
                            _logger.LogInformation("Fetched perimeter data for elder: {ElderEmail}", elder.Email);
                            elderLocations.Add(new ElderLocation
                            {
                                email = elder.Email,
                                name = elder.Name,
                                latitude = location.Latitude,
                                longitude = location.Longitude,
                                lastUpdated = location.Timestamp,
                                perimeter = new Perimeter
                                {
                                    Latitude = perimeter.Latitude,
                                    Longitude = perimeter.Longitude,
                                    Radius = perimeter.Radius
                                }
                            });
                        }
                        else
                        {
                            _logger.LogInformation("No perimeter data found for elder: {ElderEmail}", elder.Email);
                            elderLocations.Add(new ElderLocation
                            {
                                email = elder.Email,
                                name = elder.Name,
                                latitude = location.Latitude,
                                longitude = location.Longitude,
                                lastUpdated = location.Timestamp
                            });
                        }
                    }
                }
            }
            if (elderLocations.Count == 0)
            {
                _logger.LogError("No location data found for the elders.");
                return BadRequest("No location data found for the elders.");
            }
            _logger.LogInformation("Fetched location data for {ElderCount} elders.", elderLocations.Count);
            return elderLocations;
        }

        [HttpGet("Address")]
        public async Task<ActionResult<string>> GetAddress(string elderEmail)
        {
            Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
            if (elder is null)
            {
                _logger.LogError("Elder not found for email: {ElderEmail}", elderEmail);
                return BadRequest("Elder not found.");
            }
            if (string.IsNullOrEmpty(elder.Arduino))
            {
                _logger.LogError("Elder Arduino not set for elder: {ElderEmail}", elderEmail);
                return BadRequest("Elder Arduino not set.");
            }
            _logger.LogInformation("Fetching address data for elder: {ElderEmail}", elderEmail);
            Location? location = _db.Location.FirstOrDefault(m => m.MacAddress == elder.Arduino);
            if (location is null)
            {
                _logger.LogError("Location not found for elder: {ElderEmail}", elderEmail);
                return BadRequest("Location not found.");
            }
            string address = await _geoService.GetAddressFromCoordinates(location.Latitude, location.Longitude);
            _logger.LogInformation("Fetched address data for elder: {ElderEmail}", elder.Email);
            if (string.IsNullOrEmpty(address))
            {
                _logger.LogError("Address not found for elder: {ElderEmail}", elderEmail);
                return BadRequest("Address not found.");
            }
            _logger.LogInformation("Fetched address data for elder: {ElderEmail}", elder.Email);
            return address;
        }

        [HttpPost("Perimeter")]
        public async Task<ActionResult> SetPerimeter(int radius, string elderEmail)
        {
            Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
            _logger.LogInformation("Setting perimeter for elder: {ElderEmail}", elderEmail);
            if (elder is null)
            {
                _logger.LogError("Elder not found for email: {ElderEmail}", elderEmail);
                return BadRequest("Elder not found.");
            }
            if (string.IsNullOrEmpty(elder.Arduino))
            {
                _logger.LogError("Elder Arduino not set for elder: {ElderEmail}", elderEmail);
                return BadRequest("Elder Arduino not set.");
            }
            if (radius < 0)
            {
                _logger.LogError("Invalid radius value: {Radius}", radius);
                return BadRequest("Invalid radius value.");
            }
            if (elder.latitude == null || elder.longitude == null)
            {
                _logger.LogError("No home address set");
                return BadRequest("No home address set");
            }
            _logger.LogInformation("Setting perimeter for elder: {ElderEmail}", elderEmail);
            Perimeter? oldPerimeter = _db.Perimeter.FirstOrDefault(m => m.MacAddress == elder.Arduino);
            if (oldPerimeter == null)
            {
                _logger.LogInformation("Creating new perimeter for elder: {ElderEmail}", elderEmail);
                Perimeter perimeter = new Perimeter
                {
                    Latitude = elder.latitude,
                    Longitude = elder.longitude,
                    Radius = radius,
                    MacAddress = elder.Arduino
                };
                _db.Perimeter.Add(perimeter);
            }
            else
            {
                _logger.LogInformation("Updating existing perimeter for elder: {ElderEmail}", elderEmail);
                oldPerimeter = new Perimeter
                {
                    Latitude = elder.latitude,
                    Longitude = elder.longitude,
                    Radius = radius,
                    MacAddress = elder.Arduino
                };
                _db.Perimeter.Update(oldPerimeter);
                
                // Send email to caregiver
                var caregivers = await _caregiverManager.Users
                    .Where(c => c.Elders != null && c.Elders.Any(e => e.Id == elder.Id))
                    .ToListAsync();
                foreach (var caregiver in caregivers)
                {
                    var emailInfo = new Email { name = caregiver.Name, email = caregiver.Email };
                    _logger.LogInformation("Sending email to {CaregiverEmail}", caregiver.Email);
                    await _emailService.SendEmail(emailInfo, "Elder changed their perimeter", $"Elder {elder.Name} changed their perimeter to {radius} kilometers.");
                }
            }
            
            try
            {
                _logger.LogInformation("Saving perimeter data for elder: {ElderEmail}", elderEmail);
                await _db.SaveChangesAsync();
                return Ok("Perimeter set successfully");
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to set perimeter: {Error}", e.Message);
                return BadRequest("Failed to set perimeter");
            }
        }
    }
}