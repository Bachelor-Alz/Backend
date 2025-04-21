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
        
        if (data.Count == 0)
        {
            _logger.LogWarning("{Timestamp}: {SensorType} data was empty from IP: {IP}.", receivedAt, typeof(T).Name, ip);
            return new BadRequestObjectResult($"{typeof(T).Name} data is empty.");
        }
        
        Elder? elder = await _elderManager.Users.FirstOrDefaultAsync(e => e.Arduino == data.First().Address);
        
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

    public async Task HandleArduinoData(Arduino data, HttpContext httpContext)
    {
        string ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        DateTime receivedAt = DateTime.UtcNow;

        // Validate if the elder exists for the given MacAddress
        Elder? elder = await _elderManager.Users.FirstOrDefaultAsync(e => e.Arduino == data.MacAddress);
        if (elder == null)
        {
            _logger.LogWarning("{Timestamp}: No elder found with MacAddress {MacAddress} from IP: {IP}.", receivedAt, data.MacAddress, ip);
            return;
        }

        // Add GPS data
        _dbContext.GPSData.Add(new GPS
        {
            Latitude = data.Latitude,
            Longitude = data.Longitude,
            Timestamp = receivedAt,
            Address = data.MacAddress
        });
        
        Steps neweststeps = await _dbContext.Steps
            .Where(s => s.MacAddress == data.MacAddress)
            .OrderByDescending(s => s.Timestamp)
            .FirstOrDefaultAsync();
        if (neweststeps != null && neweststeps.Timestamp.Date == receivedAt.Date)
        {
            // If the newest step entry is from today, update it
            neweststeps.StepsCount += data.steps;
        }
        else
        {
            // If not, add a new entry
            _dbContext.Steps.Add(new Steps()
            {
                StepsCount = data.steps,
                Timestamp = receivedAt,
                MacAddress = data.MacAddress
            });
        }

        // Add MAX30102 data
        foreach (var entry in data.Max30102)
        {
            _dbContext.MAX30102Data.Add(new Max30102
            {
                Heartrate = entry.heartRate,
                SpO2 = entry.SpO2,
                Timestamp = receivedAt,
                Address = data.MacAddress
            });
        }

        try
        {
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("{Timestamp}: Successfully saved Arduino data from IP: {IP}.", receivedAt, ip);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Timestamp}: Error saving Arduino data from IP: {IP}.", receivedAt, ip);
        }
    }
}
