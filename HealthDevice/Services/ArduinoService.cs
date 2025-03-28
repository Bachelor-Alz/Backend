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

    public async Task<ActionResult> HandleSensorData(List<GPS> data, HttpContext httpContext)
    {
        string ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        DateTime receivedAt = DateTime.UtcNow;
        
        if (data.Count == 0)
        {
            _logger.LogWarning("{Timestamp}: GPS data was empty from IP: {IP}.", receivedAt, ip);
            return new BadRequestObjectResult("GPS data is empty.");
        }
        
        Elder? elder = await _elderManager.Users.FirstOrDefaultAsync(e => e.arduino == data.First().Address);

        foreach (GPS gps in data)
        {
            gps.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(gps.EpochTimestamp).UtcDateTime;
        }
        if (elder == null)
        {
            _dbContext.GpsData.AddRange(data);
            try
            {
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("{Timestamp}: Saved {Count} GPS entries from IP: {IP}.", receivedAt, data.Count, ip);
                return new OkResult();
            }
            catch (Exception e)
            {
                _logger.LogError("{Timestamp}: Error saving GPS entries from IP: {IP}.", receivedAt, ip);
                return new BadRequestObjectResult("Error saving GPS entries.");
            }
        }
        
        elder.gpsData.AddRange(data);
        try
        {
            await _elderManager.UpdateAsync(elder);
            _logger.LogInformation("{Timestamp}: Saved {Count} GPS entries from IP: {IP}.", receivedAt, data.Count, ip);
            return new OkResult();
        }
        catch (Exception e)
        {
            _logger.LogError("{Timestamp}: Error saving GPS entries from IP: {IP}.", receivedAt, ip);
            return new BadRequestObjectResult("Error saving GPS entries.");
        }
    }

    public async Task<ActionResult> HandleSensorData(List<Max30102> data, HttpContext httpContext)
    {
        string ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        DateTime receivedAt = DateTime.UtcNow;
        
        if (data.Count == 0)
        {
            _logger.LogWarning("{Timestamp}: MAX30102 data was empty from IP: {IP}.", receivedAt, ip);
            return new BadRequestObjectResult("MAX30102 data is empty.");
        }
        
        Elder? elder = await _elderManager.Users.FirstOrDefaultAsync(e => e.arduino == data.First().Address);

        foreach (Max30102 entry in data)
        {
            entry.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(entry.EpochTimestamp).UtcDateTime;
        }

        
        if (elder == null)
        {
            _dbContext.Max30102Data.AddRange(data);

            try
            {
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("{Timestamp}: Saved {Count} MAX30102 entries from IP: {IP}.", receivedAt, data.Count, ip);
                return new OkResult();
            }
            catch (Exception e)
            {
                _logger.LogError("{receivedAt}: Error saving MAX30102 entries from IP: {IP}.", receivedAt, ip);
                return new BadRequestObjectResult("Error saving MAX30102 entries.");
            }
        }
        
        elder.Max30102Datas.AddRange(data);

        try
        {
            await _elderManager.UpdateAsync(elder);
            _logger.LogInformation("{receivedAt}: Saved {Count} MAX30102 entries from IP: {IP}.", receivedAt, data.Count, ip);
            return new OkResult();
        }
        catch (Exception e)
        {
            _logger.LogError("{receivedAt}: Error saving MAX30102 entries from IP: {IP}.", receivedAt, ip);
            return new BadRequestObjectResult("Error saving MAX30102 entries.");
        }
    }
}
