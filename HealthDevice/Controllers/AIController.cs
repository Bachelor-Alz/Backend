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
    public async Task Compute([FromBody] List<int> predictions, string mac)
    {
        _logger.LogInformation("Amount of received predictions: {predictions} for MAC address: {mac}", predictions.Count, mac);
        if (!(predictions.Count == 0 || string.IsNullOrEmpty(mac))) 
            await _aiService.HandleAiRequest(predictions, mac);
    }
}