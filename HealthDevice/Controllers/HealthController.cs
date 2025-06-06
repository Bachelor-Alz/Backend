using System.Security.Claims;
using HealthDevice.DTO;
using HealthDevice.Models;
using HealthDevice.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Period = HealthDevice.DTO.Period;
using StepsDTO = HealthDevice.DTO.StepsDTO;

// ReSharper disable All

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
        private readonly IRepository<Elder> _elderRepository;
        private readonly IRepository<Location> _locationRepository;

        public HealthController
        (
            IHealthService healthService,
            ILogger<HealthController> logger,
            GeoService geoService,
            IRepository<Elder> elderRepository,
            IRepository<Location> locationRepository
        )
        {
            _healthService = healthService;
            _logger = logger;
            _geoService = geoService;
            _elderRepository = elderRepository;
            _locationRepository = locationRepository;
        }

        [HttpGet("Heartrate")]
        public async Task<ActionResult<List<PostHeartRate>>> GetHeartrate(string elderId, DateTime date,
            string timezone = "Europe/Copenhagen",
            string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(periodEnum))
            {
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            return await _healthService.GetHeartrate(elderId, date, periodEnum, timeZoneInfo);
        }

        [HttpGet("Spo2")]
        public async Task<ActionResult<List<PostSpO2>>> GetSpo2(string elderId, DateTime date,
            string timezone = "Europe/Copenhagen", string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(periodEnum))
            {
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            return await _healthService.GetSpO2(
                elderId, date.ToUniversalTime(), periodEnum, timeZoneInfo);
        }

        [HttpGet("Distance")]
        public async Task<ActionResult<List<DistanceInfoDTO>>> GetDistance(string elderId, DateTime date,
            string timezone = "Europe/Copenhagen", string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(periodEnum))
            {
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            return await _healthService.GetDistance(
                elderId, date.ToUniversalTime(), periodEnum, timeZoneInfo);
        }

        [HttpGet("Steps")]
        public async Task<ActionResult<List<StepsDTO>>> GetSteps(string elderId, DateTime date,
            string timezone = "Europe/Copenhagen", string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(periodEnum))
            {
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            return await _healthService.GetSteps(
                elderId, date.ToUniversalTime(), periodEnum, timeZoneInfo);
        }

        [HttpGet("Dashboard")]
        public async Task<ActionResult<DashBoard>> GetDashBoardInfo(string elderId)
        {
            Elder? elder = await _elderRepository.Query().FirstOrDefaultAsync(m => m.Id == elderId);
            if (elder is null || string.IsNullOrEmpty(elder.MacAddress))
            {
                _logger.LogError("Elder not found or Arduino not set for Email: {ElderEmail}", elderId);
                return new DashBoard
                {
                    FallCount = 0,
                    Distance = 0,
                    HeartRate = 0,
                    SpO2 = 0,
                    Steps = 0
                };
            }

            _logger.LogInformation("Fetching dashboard data for elder: {ElderEmail}", elderId);
            return await _healthService.GetDashboardData(elder.MacAddress);
        }

        [HttpGet("Falls")]
        public async Task<ActionResult<List<FallDTO>>> GetFalls(string elderId, DateTime date,
            string timezone = "Europe/Copenhagen", string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(periodEnum))
            {
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            return await _healthService.GetFalls(elderId, date.ToUniversalTime(), periodEnum, timeZoneInfo);
        }


        [HttpGet("Coordinates")]
        [Authorize(Roles = "Elder")]
        public async Task<ActionResult<PerimeterDTO>> GetLocation()
        {
            Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
                return BadRequest("User claim is not available.");

            _logger.LogInformation("Fetching elder location for: {ElderEmail}", userClaim.Value);
            return await _healthService.GetElderPerimeter(userClaim.Value);
        }

        [HttpGet("Coordinates/Elders")]
        [Authorize(Roles = "Caregiver")]
        public async Task<ActionResult<List<ElderLocationDTO>>> GetEldersLocation()
        {
            Claim? userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
                return BadRequest("User claim is not available.");

            _logger.LogInformation("Fetching elder locations for caregiver: {CaregiverEmail}", userClaim.Value);
            return await _healthService.GetEldersLocation(userClaim.Value);
        }

        [HttpGet("Address")]
        public async Task<ActionResult<string>> GetAddress(string elderId)
        {
            Elder? elder = await _elderRepository.Query().FirstOrDefaultAsync(m => m.Id == elderId);
            if (elder is null || string.IsNullOrEmpty(elder.MacAddress))
            {
                _logger.LogError("Elder not found with: {ElderEmail} and Arduino: {mac}", elderId, elder?.MacAddress);
                return BadRequest("Elder not found.");
            }

            Location? location =
                await _locationRepository.Query().FirstOrDefaultAsync(m => m.MacAddress == elder.MacAddress);
            if (location is null)
                return BadRequest("Location not found.");

            string address = await _geoService.GetAddressFromCoordinates(location.Latitude, location.Longitude);

            if (string.IsNullOrEmpty(address))
                return BadRequest("Address not found.");

            _logger.LogInformation("Fetched address data for elder: {ElderEmail}", elder.Email);
            return address;
        }

        [HttpPost("Perimeter")]
        public async Task<ActionResult> SetPerimeter(int radius, string elderId)
        {
            return await _healthService.SetPerimeter(radius, elderId);
        }
    }
}