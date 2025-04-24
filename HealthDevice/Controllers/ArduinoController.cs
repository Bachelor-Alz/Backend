using HealthDevice.DTO;
using HealthDevice.Services;
using Microsoft.AspNetCore.Mvc;

namespace HealthDevice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ArduinoController : ControllerBase
{
    private readonly ArduinoService _arduinoService;
    private readonly ILogger<ArduinoController> _logger;

    public ArduinoController(ArduinoService arduinoService, ILogger<ArduinoController> logger)
    {
        _logger = logger;
        _arduinoService = arduinoService;
    }
    
    [HttpPost("gps")]
    public async Task<ActionResult> PostGps([FromBody] List<GPS> data)
    {
        if(data.Count == 0)
        {
            _logger.LogWarning("Received empty GPS data.");
            return BadRequest("GPS data cannot be empty.");
        }
        if(string.IsNullOrEmpty(data.First().Address))
        {
            _logger.LogWarning("Received empty MAC address.");
            return BadRequest("MAC address cannot be empty.");
        }
        _logger.LogInformation("Received GPS data: {data}", data);
        return await _arduinoService.HandleSensorData(data, HttpContext);
    }

    [HttpPost("max30102")]
    public async Task<ActionResult> PostMax30102([FromBody] List<Max30102> data)
    {
        if(data.Count == 0)
        {
            _logger.LogWarning("Received empty Max30102 data.");
            return BadRequest("Max30102 data cannot be empty.");
        }
        if(string.IsNullOrEmpty(data.First().Address))
        {
            _logger.LogWarning("Received empty MAC address.");
            return BadRequest("MAC address cannot be empty.");
        }
        _logger.LogInformation("Received Max30102 data: {data}", data);
        return await _arduinoService.HandleSensorData(data, HttpContext);
    }
    
    [HttpPost("data")]
    public async Task PostData([FromBody] Arduino data)
    {
        if(string.IsNullOrEmpty(data.MacAddress))
        {
            _logger.LogWarning("Received empty MAC address.");
        }
        _logger.LogInformation("Received Arduino data: {data}", data);
        await _arduinoService.HandleArduinoData(data, HttpContext);
    }
}