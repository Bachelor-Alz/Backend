using HealthDevice.Services;
using Microsoft.AspNetCore.Mvc;
namespace HealthDevice.Controllers;

[ApiController]
[Route("[controller]")]
public class AiController : ControllerBase
{
    private readonly AiService _aiService;
    
    public AiController(AiService aiService)
    {
        _aiService = aiService;
    }
    
    [HttpPost("compute")]
    public async Task<ActionResult> Compute([FromBody] List<int> predictions, [FromBody] string mac)
    {
        await _aiService.HandleAiRequest(predictions, mac);
        return Ok();
    }
    
}