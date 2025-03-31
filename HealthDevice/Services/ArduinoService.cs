using HealthDevice.Data;
using HealthDevice.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
// ReSharper disable All

namespace HealthDevice.Services;

public class ArduinoService(
    ILogger<ArduinoService> logger,
    UserManager<Elder> elderManager,
    ApplicationDbContext dbContext)
{
    public async Task<ActionResult> HandleSensorData<T>(List<T> data, HttpContext httpContext) where T : Sensor
    {
        string ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        DateTime receivedAt = DateTime.UtcNow;
        
        if (data.Count == 0)
        {
            logger.LogWarning("{Timestamp}: {SensorType} data was empty from IP: {IP}.", receivedAt, typeof(T).Name, ip);
            return new BadRequestObjectResult($"{typeof(T).Name} data is empty.");
        }
        
        Elder? elder = await elderManager.Users.FirstOrDefaultAsync(e => e.Arduino == data.First().Address);
        
        foreach (var entry in data)
        {
            entry.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(entry.EpochTimestamp).UtcDateTime;
        }
        
        if (elder == null)
        {
            dbContext.Set<T>().AddRange(data);
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
                await dbContext.SaveChangesAsync();
            else
                await elderManager.UpdateAsync(elder);
            
            logger.LogInformation("{Timestamp}: Saved {Count} {SensorType} entries from IP: {IP}.", receivedAt, data.Count, typeof(T).Name, ip);
            return new OkResult();
        }
        catch (Exception)
        {
            logger.LogError("{Timestamp}: Error saving {SensorType} entries from IP: {IP}.", receivedAt, typeof(T).Name, ip);
            return new BadRequestObjectResult($"Error saving {typeof(T).Name} entries.");
        }
    }
}
