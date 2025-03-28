using HealthDevice.DTO;
using HealthDevice.Services;
using Microsoft.AspNetCore.Mvc;

namespace HealthDevice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ArduinoController : ControllerBase
{
    private readonly ArduinoService _arduinoService;

    public ArduinoController(ArduinoService arduinoService)
    {
        _arduinoService = arduinoService;
    }

    [HttpPost("gps")]
    public async Task<ActionResult> PostGps([FromBody] List<GPS> data)
    {
        return await _arduinoService.HandleSensorData(data, HttpContext);
    }

    [HttpPost("max30102")]
    public async Task<ActionResult> PostMax30102([FromBody] List<Max30102> data)
    {
        return await _arduinoService.HandleSensorData(data, HttpContext);
    }
}