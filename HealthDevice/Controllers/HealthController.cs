using System.Security.Claims;
using HealthDevice.DTO;
using HealthDevice.Models;
using HealthDevice.Services;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IHealthService _healthService;
        private readonly ILogger<HealthController> _logger;
        private readonly GeoService _geoService;
        private readonly IRepositoryFactory _repositoryFactory;
        
        public HealthController(IHealthService healthService, ILogger<HealthController> logger, GeoService geoService, IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
            _geoService = geoService;
            _logger = logger;
            _healthService = healthService;
        }
        [HttpGet("Heartrate")]
        public async Task<ActionResult<List<Heartrate>>> GetHeartrate(string elderEmail, DateTime date, string timezone = "Europe/Copenhagen",
            string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(periodEnum))
            {
                _logger.LogError("Invalid period specified: {Period}", period);
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            return await _healthService.GetHeartrate(elderEmail, date,  periodEnum, timeZoneInfo);
        }

        [HttpGet("Spo2")]
        public async Task<ActionResult<List<Spo2>>> GetSpo2(string elderEmail, DateTime date, string timezone = "Europe/Copenhagen", string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(periodEnum))
            {
                _logger.LogError("Invalid period specified: {Period}", period);
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            return await _healthService.GetSpO2(
                elderEmail, date.ToUniversalTime(), periodEnum, timeZoneInfo);
        }

        [HttpGet("Distance")]
public async Task<ActionResult<List<DistanceInfo>>> GetDistance(string elderEmail, DateTime date, string timezone = "Europe/Copenhagen", string period = "Hour")
{
    if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(periodEnum))
    {
        _logger.LogError("Invalid period specified: {Period}", period);
        return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
    }
    TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
    return await _healthService.GetDistance(
        elderEmail, date.ToUniversalTime(),  periodEnum, timeZoneInfo);
}
        
       [HttpGet("Steps")]
public async Task<ActionResult<List<Steps>>> GetSteps(string elderEmail, DateTime date, string timezone = "Europe/Copenhagen", string period = "Hour")
{
    if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(periodEnum))
    {
        _logger.LogError("Invalid period specified: {Period}", period);
        return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
    }
    TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
    return await _healthService.GetSteps(
        elderEmail, date.ToUniversalTime(),  periodEnum, timeZoneInfo);
}

        [HttpGet("Dashboard")]
        public async Task<ActionResult<DashBoard>> GetDashBoardInfo(string elderEmail)
        {
            IRepository<Elder> elderRepository = _repositoryFactory.GetRepository<Elder>();
            IRepository<Max30102> max30102Repository = _repositoryFactory.GetRepository<Max30102>();
            IRepository<DistanceInfo> kilometerRepository = _repositoryFactory.GetRepository<DistanceInfo>();
            IRepository<Steps> stepsRepository = _repositoryFactory.GetRepository<Steps>();
            IRepository<FallInfo> fallInfoRepository = _repositoryFactory.GetRepository<FallInfo>();
            DateTime currentDate = DateTime.UtcNow;
            Elder? elder = await elderRepository.Query().FirstOrDefaultAsync(m => m.Email == elderEmail);
            if (elder is null || string.IsNullOrEmpty(elder.MacAddress))
            {
                _logger.LogError("Elder not found or Arduino not set for email: {ElderEmail}", elderEmail);
                return new DashBoard
                {
                    allFall = 0,
                    distance = 0,
                    HeartRate = 0,
                    SpO2 = 0,
                    steps = 0
                };
            }
            _logger.LogInformation("Fetching dashboard data for elder: {ElderEmail}", elderEmail);

            string macAddress = elder.MacAddress;

            // Query data objects using the MacAddress
            Max30102? max30102 = await max30102Repository.Query()
                .Where(m => m.MacAddress == macAddress && m.Timestamp.Date == currentDate.Date)
                .OrderByDescending(m => m.Timestamp)
                .FirstOrDefaultAsync();

            //Get the total amounts of steps on the newest date using kilometer
            DistanceInfo? kilometer = await kilometerRepository.Query().Where(s => s.MacAddress == macAddress && s.Timestamp.Date == currentDate.Date)
                .GroupBy(s => s.Timestamp.Date)
                .Select(g => new DistanceInfo
                {
                    Distance = g.Sum(s => s.Distance),
                    Timestamp = g.Key
                }).FirstOrDefaultAsync();

            Steps? steps = await stepsRepository.Query()
                .Where(s => s.MacAddress == macAddress && s.Timestamp.Date == currentDate.Date)
                .GroupBy(s => s.Timestamp.Date)
                .Select(g => new Steps
                {
                    StepsCount = g.Sum(s => s.StepsCount),
                    Timestamp = g.Key
                }).FirstOrDefaultAsync();
            
            _logger.LogInformation("Fetched data for elder: {ElderEmail}", elderEmail);
            
            return new DashBoard
            {
                allFall = fallInfoRepository.Query().Where(t => t.Timestamp.Date == currentDate.Date).Count(f => f.MacAddress == macAddress),
                distance = kilometer?.Distance ?? 0,
                HeartRate = max30102?.LastHeartrate ?? 0,
                SpO2 = max30102?.LastSpO2 ?? 0,
                steps = steps?.StepsCount ?? 0
            };
        }

        [HttpGet("Falls")]
        public async Task<ActionResult<List<FallDTO>>> GetFalls(string elderEmail, DateTime date,
            string timezone = "Europe/Copenhagen", string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(periodEnum))
            {
                _logger.LogError("Invalid period specified: {Period}", period);
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            } 
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            return await _healthService.GetFalls(elderEmail, date.ToUniversalTime(), periodEnum, timeZoneInfo);
        }
        

        [HttpGet("Coordinates")]
        public async Task<ActionResult<Location>> GetLocaiton(string elderEmail)
        {
            IRepository<Elder> elderRepository = _repositoryFactory.GetRepository<Elder>();
            IRepository<Location> locationRepository = _repositoryFactory.GetRepository<Location>();
            Elder? elder = await elderRepository.Query().FirstOrDefaultAsync(m => m.Email == elderEmail);
            if (elder is null)
            {
                _logger.LogError("Elder not found for email: {ElderEmail}", elderEmail);
                return BadRequest("Elder not found.");
            }
            if (string.IsNullOrEmpty(elder.MacAddress))
            {
                _logger.LogError("Elder Arduino not set for elder: {ElderEmail}", elderEmail);
                return BadRequest("Elder Arduino not set.");
            }
            _logger.LogInformation("Fetching location data for elder: {ElderEmail}", elderEmail);
            Location? location = await locationRepository.Query().FirstOrDefaultAsync(m => m.MacAddress == elder.MacAddress);
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
        public async Task<ActionResult<List<ElderLocationDTO>>> GetEldersLocation()
        {
            Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            {
                _logger.LogError("User claim is null or empty.");
                return BadRequest("User claim is not available.");
            }
            _logger.LogInformation("Fetching elder locations for caregiver: {CaregiverEmail}", userClaim.Value);
            return await _healthService.GetEldersLocation(userClaim.Value);
        }

        [HttpGet("Address")]
        public async Task<ActionResult<string>> GetAddress(string elderEmail)
        {
            IRepository<Elder> elderRepository = _repositoryFactory.GetRepository<Elder>();
            IRepository<Location> locationRepository = _repositoryFactory.GetRepository<Location>();
            Elder? elder = await elderRepository.Query().FirstOrDefaultAsync(m => m.Email == elderEmail);
            if (elder is null)
            {
                _logger.LogError("Elder not found for email: {ElderEmail}", elderEmail);
                return BadRequest("Elder not found.");
            }
            if (string.IsNullOrEmpty(elder.MacAddress))
            {
                _logger.LogError("Elder Arduino not set for elder: {ElderEmail}", elderEmail);
                return BadRequest("Elder Arduino not set.");
            }
            _logger.LogInformation("Fetching address data for elder: {ElderEmail}", elderEmail);
            Location? location = await locationRepository.Query().FirstOrDefaultAsync(m => m.MacAddress == elder.MacAddress);
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
            return await _healthService.SetPerimeter(radius, elderEmail);
        }
    }
}