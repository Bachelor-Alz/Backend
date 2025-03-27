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
        public async Task<ActionResult<Heartrate>> GetHeartrate(string elderEmail)
        {
            DateTime currenttime = DateTime.Now;
            Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
            
            if(elder == null)
            {
                return BadRequest();
            }

            return await _healthService.CalculateHeartRate(currenttime, elder);
        }

        [HttpGet("SpO2")]
        [Authorize]
        public async Task<ActionResult<Spo2>> GetSpO2(string elderEmail)
        {
            DateTime currenttime = DateTime.Now;
            Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);

            if(elder == null)
            {
                return BadRequest();
            }
            
            return await _healthService.CalculateSpo2(currenttime, elder);
        }
}
}
