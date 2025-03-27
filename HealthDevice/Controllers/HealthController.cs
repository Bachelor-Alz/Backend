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
        public async Task<ActionResult<Heartrate>> GetHeartrateHourly(string elderEmail)
        {
            DateTime currentTime = DateTime.Now;
            Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
            
            if(elder == null)
            {
                _logger.LogError("No elder found with email {email}", elderEmail);
                return BadRequest();
            }
            
            Heartrate heartRate = await _healthService.CalculateHeartRate(currentTime, elder);
            elder.heartRates.Add(heartRate);

            try
            {
                await _elderManager.UpdateAsync(elder);
                return heartRate;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error updating elder with email {email}", elderEmail);
                return BadRequest();
            }
            
        }

        [HttpGet("heartrate/all")]
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

        [HttpGet("SpO2")]
        [Authorize]
        public async Task<ActionResult<Spo2>> GetSpO2Hourly(string elderEmail)
        {
            DateTime currentTime = DateTime.Now;
            Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);

            if(elder == null)
            {
                _logger.LogError("No elder found with email {email}", elderEmail);
                return BadRequest();
            }
            
            Spo2 spo2 = await _healthService.CalculateSpo2(currentTime, elder);
            elder.spo2s.Add(spo2);

            try
            {
                await _elderManager.UpdateAsync(elder);
                return spo2;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error updating elder with email {email}", elderEmail);
                return BadRequest();
            }
        }
        
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
}
}
