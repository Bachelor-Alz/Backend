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
    public async Task<IActionResult> Compute([FromBody] object data)
    {
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("http://localhost:5000/compute", content);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();
        return Ok(result);
    }
}