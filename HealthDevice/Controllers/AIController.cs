using HealthDevice.Models;

namespace HealthDevice.Controllers;

using System.Net.Http;
using System.Text;
using System.Text.Json;
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
    public async Task<IActionResult> Compute()
    {
        return null;
    }

    [HttpGet("fall")]
    public async Task<IActionResult> Fall()
    {
        return null;
    }
}