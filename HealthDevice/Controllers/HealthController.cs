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

        public HealthController(UserManager<Elder> elderManager, HealthService healthService)
        {
            _elderManager = elderManager;
            _healthService = healthService;
        }

        [HttpGet("Heartrate")]
        public async Task<ActionResult<List<Heartrate>>> GetHeartrate(string elderEmail, DateTime date, Period period = Period.Hour)
        {
            return await _healthService.GetHealthData<Heartrate>(elderEmail, period, date, e => e.heartRates, _elderManager);
        }

        [HttpGet("Spo2")]
        public async Task<ActionResult<List<Spo2>>> GetSpo2(string elderEmail, DateTime date, Period period = Period.Hour)
        {
            return await _healthService.GetHealthData<Spo2>(elderEmail, period, date, e => e.spo2s, _elderManager);
        }
    }
}