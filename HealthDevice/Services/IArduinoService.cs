using HealthDevice.DTO;
using Microsoft.AspNetCore.Mvc;

namespace HealthDevice.Services;

public interface IArduinoService
{
    Task<ActionResult> HandleSensorData<T>(List<T> data, HttpContext httpContext) where T : Sensor;
    Task HandleArduinoData(ArduinoDTO data, HttpContext httpContext);
}