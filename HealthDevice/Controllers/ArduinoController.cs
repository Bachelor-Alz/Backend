using HealthDevice.DTO;
using HealthDevice.Services;
using Microsoft.AspNetCore.Mvc;

namespace HealthDevice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ArduinoController(ArduinoService arduinoService) : ControllerBase
{
    [HttpPost("gps")]
    public async Task<ActionResult> PostGps([FromBody] List<GPS> data)
    {
        return await arduinoService.HandleSensorData(data, HttpContext);
    }

    [HttpPost("max30102")]
    public async Task<ActionResult> PostMax30102([FromBody] List<Max30102> data)
    {
        return await arduinoService.HandleSensorData(data, HttpContext);
    }
}