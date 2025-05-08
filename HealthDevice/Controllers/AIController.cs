using HealthDevice.Services;
using Microsoft.AspNetCore.Mvc;

namespace HealthDevice.Controllers;

[ApiController]
[Route("[controller]")]
public class AiController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly ILogger<AiController> _logger;

    public AiController(IAIService aiService, ILogger<AiController> logger)
    {
        _logger = logger;
        _aiService = aiService;
    }

    [HttpPost("compute")]
    public async Task<ActionResult> Compute([FromBody] List<int> predictions, string mac)
    {
        if (predictions.Count == 0)
        {
            _logger.LogWarning("Received empty predictions list.");
            return BadRequest("Predictions list cannot be empty.");
        }
        if (string.IsNullOrEmpty(mac))
        {
            _logger.LogWarning("Received empty MAC address.");
            return BadRequest("MAC address cannot be empty.");
        }
        _logger.LogInformation("Received predictions: {predictions} for MAC address: {mac}", predictions, mac);
        await _aiService.HandleAiRequest(predictions, mac);
        return Ok();
    }
}