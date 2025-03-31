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

        public HealthController(UserManager<Elder> elderManager, HealthService healthService)
        {
            _elderManager = elderManager;
            _healthService = healthService;
        }

        [HttpGet("Heartrate")]
        public async Task<ActionResult<List<Heartrate>>> GetHeartrate(string elderEmail, DateTime date, string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(typeof(Period), periodEnum))
            {
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }
            return await _healthService.GetHealthData<Heartrate>(elderEmail, periodEnum, date, e => e.heartRate, _elderManager);
        }

        [HttpGet("Spo2")]
        public async Task<ActionResult<List<Spo2>>> GetSpo2(string elderEmail, DateTime date, string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(typeof(Period), periodEnum))
            {
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }
            return await _healthService.GetHealthData<Spo2>(elderEmail, periodEnum, date, e => e.spo2s, _elderManager);
        }
        
        [HttpGet("Distance")]
        public async Task<ActionResult<List<Kilometer>>> GetDistance(string elderEmail, DateTime date, string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(typeof(Period), periodEnum))
            {
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }
            return await _healthService.GetHealthData<Kilometer>(elderEmail, periodEnum, date, e => e.distance, _elderManager);
        }
        
        [HttpGet("Steps")]
        public async Task<ActionResult<List<Steps>>> GetSteps(string elderEmail, DateTime date, string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(typeof(Period), periodEnum))
            {
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }
            return await _healthService.GetHealthData<Steps>(elderEmail, periodEnum, date, e => e.steps, _elderManager);
        }
    }
}
