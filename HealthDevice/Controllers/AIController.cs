using System.Runtime.InteropServices.JavaScript;
using HealthDevice.DTO;
using HealthDevice.Services;

namespace HealthDevice.Controllers;

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class AIController : ControllerBase
{
    private readonly ILogger<AIController> _logger;
    private readonly AIService _aiService;
    
    public AIController(ILogger<AIController> logger, AIService aiService)
    {
        _logger = logger;
        _aiService = aiService;
    }

    [HttpPost("compute")]
    public async Task<ActionResult> Compute([FromBody] List<int> data)
    {
        if (data == null)
        {
            return BadRequest();
        }
        await _aiService.HandleAIRequest(data);
        return Ok();
    }
    
}