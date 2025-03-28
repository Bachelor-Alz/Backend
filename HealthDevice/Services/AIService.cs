using HealthDevice.DTO;
using Microsoft.AspNetCore.Identity;

namespace HealthDevice.Services;

public class AIService
{
    private readonly UserManager<Elder> _elderManager;
    
    public AIService(UserManager<Elder> elderManager)
    {
        _elderManager = elderManager;
    }
    
    public void HandleAIRequest(List<int> request)
    {
        Console.WriteLine(request);
    }
}