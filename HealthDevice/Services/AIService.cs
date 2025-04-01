using HealthDevice.DTO;
using Microsoft.AspNetCore.Mvc;

namespace HealthDevice.Services;

public class AiService
{
    
    private readonly ILogger<AiService> _logger;
    
    public AiService(ILogger<AiService> logger)
    {
        _logger = logger;
    }
    public Task<ActionResult> HandleAiRequest([FromBody] List<int> request)
    {
       _logger.LogInformation("HandleAIRequest {request}", request);
       if (request.Contains(1))
       {
           HandleFall();
       }
       
       return Task.FromResult<ActionResult>(new OkResult());
    }

    private static void HandleFall()
    {
        
        FallInfo fallInfo = new FallInfo()
        {
            Timestamp = DateTime.Now,
            Location = new Location(),
        };
    }
}