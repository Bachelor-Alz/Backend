using HealthDevice.DTO;
using HealthDevice.Models;
using HealthDevice.Services;
using Microsoft.AspNetCore.Mvc;

namespace HealthDevice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ArduinoController : ControllerBase
{
    private readonly IArduinoService _arduinoService;
    private readonly ILogger<ArduinoController> _logger;

    public ArduinoController(IArduinoService arduinoService, ILogger<ArduinoController> logger)
    {
        _logger = logger;
        _arduinoService = arduinoService;
    }

    [HttpPost("gps")]
    public async Task PostGps([FromBody] List<GPSData> data)
    {
        _logger.LogInformation("Received {data} GPS data, with MacAddress {}", data.Count, data.First().MacAddress);
        if (!(data.Count == 0 || string.IsNullOrEmpty(data.First().MacAddress)))
            await _arduinoService.HandleSensorData(data, HttpContext);
    }

    [HttpPost("data")]
    public async Task PostData([FromBody] ArduinoDTO data)
    {
        _logger.LogInformation("Received Arduino data: {data}", data);
        if (!string.IsNullOrEmpty(data.MacAddress))
            await _arduinoService.HandleArduinoData(data, HttpContext);
    }
}