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
    public class HealthController(UserManager<Elder> elderManager, HealthService healthService)
        : ControllerBase
    {
        [HttpGet("Heartrate")]
        public async Task<ActionResult<List<Heartrate>>> GetHeartrate(string elderEmail, DateTime date, string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(typeof(Period), periodEnum))
            {
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }
            return await healthService.GetHealthData<Heartrate>(elderEmail, periodEnum, date, e => e.Heartrate, elderManager);
        }

        [HttpGet("Spo2")]
        public async Task<ActionResult<List<Spo2>>> GetSpo2(string elderEmail, DateTime date, string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(typeof(Period), periodEnum))
            {
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }
            return await healthService.GetHealthData<Spo2>(elderEmail, periodEnum, date, e => e.SpO2, elderManager);
        }
        
        [HttpGet("Distance")]
        public async Task<ActionResult<List<Kilometer>>> GetDistance(string elderEmail, DateTime date, string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(typeof(Period), periodEnum))
            {
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }
            return await healthService.GetHealthData<Kilometer>(elderEmail, periodEnum, date, e => e.Distance, elderManager);
        }
        
        [HttpGet("Steps")]
        public async Task<ActionResult<List<Steps>>> GetSteps(string elderEmail, DateTime date, string period = "Hour")
        {
            if (!Enum.TryParse<Period>(period, true, out var periodEnum) || !Enum.IsDefined(typeof(Period), periodEnum))
            {
                return BadRequest("Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
            }
            return await healthService.GetHealthData<Steps>(elderEmail, periodEnum, date, e => e.Steps, elderManager);
        }
    }
}
