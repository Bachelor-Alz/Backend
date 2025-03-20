using HealthDevice.DTO;

namespace HealthDevice.Controllers;

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class AIController : ControllerBase
{
    private readonly HttpClient _httpClient;

    public AIController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [HttpPost("compute")]
    public async Task<ActionResult<FallInfo>> Compute()
    {
        return null;
    }
    
}