using HealthDevice.Data;
using HealthDevice.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthDevice.Services;

public class ArduinoService
{
    private readonly ILogger<ArduinoService> _logger;
    private readonly UserManager<Elder> _elderManager;
    private readonly ApplicationDbContext _dbContext;

    public ArduinoService(ILogger<ArduinoService> logger, UserManager<Elder> elderManager, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _elderManager = elderManager;
        _dbContext = dbContext;
    }

    public async Task<ActionResult> HandleSensorData<T>(List<T> data, HttpContext httpContext) where T : Sensor
    {
        string ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        DateTime receivedAt = DateTime.UtcNow;
        
        if (!data.Any())
        {
            _logger.LogWarning("{Timestamp}: {SensorType} data was empty from IP: {IP}.", receivedAt, typeof(T).Name, ip);
            return new BadRequestObjectResult($"{typeof(T).Name} data is empty.");
        }
        
        Elder? elder = await _elderManager.Users.FirstOrDefaultAsync(e => e.arduino == data.First().Address);
        
        foreach (var entry in data)
        {
            entry.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(entry.EpochTimestamp).UtcDateTime;
        }
        
        if (elder == null)
        {
            _dbContext.Set<T>().AddRange(data);
        }
        else
        {
            if (typeof(Elder).GetProperty(typeof(T).Name)?.GetValue(elder) is List<T> elderDataList)
            {
                elderDataList.AddRange(data);
            }
        }

        try
        {
            if (elder == null)
                await _dbContext.SaveChangesAsync();
            else
                await _elderManager.UpdateAsync(elder);
            
            _logger.LogInformation("{Timestamp}: Saved {Count} {SensorType} entries from IP: {IP}.", receivedAt, data.Count, typeof(T).Name, ip);
            return new OkResult();
        }
        catch (Exception)
        {
            _logger.LogError("{Timestamp}: Error saving {SensorType} entries from IP: {IP}.", receivedAt, typeof(T).Name, ip);
            return new BadRequestObjectResult($"Error saving {typeof(T).Name} entries.");
        }
    }
}
