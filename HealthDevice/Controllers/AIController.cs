using HealthDevice.DTO;
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
    public async Task Compute([FromBody] AiRequest request)
    {
        _logger.LogInformation("Amount of received predictions: {predictions} for MAC address: {mac}",
            request.Predictions.Count, request.Mac);
        if (!(request.Predictions.Count == 0 || string.IsNullOrEmpty(request.Mac)))
            await _aiService.HandleAiRequest(request.Predictions, request.Mac);
    }
}