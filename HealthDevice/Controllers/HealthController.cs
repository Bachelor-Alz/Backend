using HealthDevice.Data;
using HealthDevice.DTO;
using HealthDevice.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthDevice.Controllers
{
    public enum Period
    {
        Hour,
        Day,
        Week,
        Month
    }
    
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HealthController : ControllerBase
    {
        private readonly UserManager<Elder> _elderManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HealthController> _logger;
        private readonly UserManager<Caregiver> _caregiverManager;
        private readonly HealthService _healthService;

        public HealthController(UserManager<Elder> elderManager, ApplicationDbContext context,
            UserManager<Caregiver> caregiverManager, ILogger<HealthController> logger, HealthService healthService)
        {
            _elderManager = elderManager;
            _context = context;
            _logger = logger;
            _caregiverManager = caregiverManager;
            _healthService = healthService;
        }

        [HttpGet("Heartrate")]
        public async Task<ActionResult<List<Heartrate>>> GetHeartrate(string elderEmail, DateTime date, Period period = Period.Hour)
        {
            return await GetHealthData<Heartrate>(elderEmail, period, date, e => e.heartRates);
        }

        [HttpGet("Spo2")]
        public async Task<ActionResult<List<Spo2>>> GetSpo2(string elderEmail, DateTime date, Period period = Period.Hour)
        {
            return await GetHealthData<Spo2>(elderEmail, period, date, e => e.spo2s);
        }

        private async Task<ActionResult<List<T>>> GetHealthData<T>(string elderEmail, Period period, DateTime date, Func<Elder, List<T>> selector) where T : class
        {
            DateTime olDateTime = period switch
            {
                Period.Hour => date - TimeSpan.FromHours(1),
                Period.Day => date - TimeSpan.FromDays(1),
                Period.Week => date - TimeSpan.FromDays(7),
                Period.Month => date - TimeSpan.FromDays(30),
                _ => throw new ArgumentException("Invalid period specified")
            };

            Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
            if (elder == null)
            {
                _logger.LogError("No elder found with email {email}", elderEmail);
                return BadRequest();
            }

            List<T> data = selector(elder).Where(d => ((dynamic)d).Timestamp >= olDateTime && ((dynamic)d).Timestamp <= date).ToList();
            return data;
        }
    }
}
