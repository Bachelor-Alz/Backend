using System.Runtime.InteropServices.JavaScript;
using HealthDevice.DTO;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthDevice.Services;

public class AIService
{
    private readonly UserManager<Elder> _elderManager;
    private readonly ILogger<AIService> _logger;
    
    public AIService(UserManager<Elder> elderManager, ILogger<AIService> logger)
    {
        _elderManager = elderManager;
        _logger = logger;
    }
    
    public Task<ActionResult> HandleAIRequest([FromBody] List<int> request)
    {
       _logger.LogInformation("HandleAIRequest {request}", request);
       if (request.Contains(1))
       {
           HandleFall();
       }
       
       return Task.FromResult<ActionResult>(new OkResult());
    }

    public void HandleFall()
    {
        
        FallInfo fallInfo = new FallInfo()
        {
            timestamp = DateTime.Now,
            location = new Location(),
        };
    }
}