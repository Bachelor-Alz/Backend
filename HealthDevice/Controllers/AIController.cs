using HealthDevice.Services;
using Microsoft.AspNetCore.Mvc;
namespace HealthDevice.Controllers;

[ApiController]
[Route("[controller]")]
public class AiController(AiService aiService) : ControllerBase
{
    [HttpPost("compute")]
    public async Task<ActionResult> Compute([FromBody] List<int> data)
    {
        await aiService.HandleAiRequest(data);
        return Ok();
    }
    
}