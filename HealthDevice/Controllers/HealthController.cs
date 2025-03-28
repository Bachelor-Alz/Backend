using HealthDevice.Data;
using HealthDevice.DTO;
using HealthDevice.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthDevice.Controllers
{
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

        [HttpGet("heartrate")]
        [Authorize]
        public async Task<ActionResult<List<Heartrate>>> GetAllHeartrate(string elderEmail)
        {
            Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
            if(elder == null)
            {
                _logger.LogError("No elder found with email {email}", elderEmail);
                return BadRequest();
            }
            return elder.heartRates;
        }
        
        [HttpGet("Spo2")]
        public async Task<ActionResult<List<Spo2>>> GetAllSpO2(string elderEmail)
        {
            Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
            if(elder == null)
            {
                _logger.LogError("No elder found with email {email}", elderEmail);
                return BadRequest();
            }
            return elder.spo2s;
        }
        
        [HttpGet("Heartrate/hour")]
        public async Task<ActionResult<List<Heartrate>>> GetHeartrateHour(string elderEmail, DateTime date)
        {
            DateTime olDateTime = date - TimeSpan.FromHours(1);
            Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
            if(elder == null)
            {
                _logger.LogError("No elder found with email {email}", elderEmail);
                return BadRequest();
            }
            List<Heartrate> heartrates = elder.heartRates.Where(d => d.Timestamp >= olDateTime && d.Timestamp <= date).ToList();
            return heartrates;
        }
        
        [HttpGet("Spo2/hour")]
        public async Task<ActionResult<List<Spo2>>> GetSpo2Hour(string elderEmail, DateTime date)
        {
            DateTime olDateTime = date - TimeSpan.FromHours(1);
            Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
            if(elder == null)
            {
                _logger.LogError("No elder found with email {email}", elderEmail);
                return BadRequest();
            }
            List<Spo2> spo2s = elder.spo2s.Where(d => d.Timestamp >= olDateTime && d.Timestamp <= date).ToList();
            return spo2s;
        }
}
}
