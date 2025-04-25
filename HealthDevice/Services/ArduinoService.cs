using HealthDevice.Data;
using HealthDevice.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthDevice.Services;

public class ArduinoService : IArduinoService
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
        _logger.LogInformation("{Timestamp}: Received {SensorType} data from IP: {IP}.", receivedAt, typeof(T).Name, ip);
        
        if (data.Count == 0)
        {
            _logger.LogWarning("{Timestamp}: {SensorType} data was empty from IP: {IP}.", receivedAt, typeof(T).Name, ip);
            return new BadRequestObjectResult($"{typeof(T).Name} data is empty.");
        }
        
            _logger.LogInformation("{Timestamp}: No elder found with MacAddress {MacAddress} from IP: {IP}.", receivedAt, data.First().MacAddress, ip);
            _dbContext.Set<T>().AddRange(data);
        try
        {
                await _dbContext.SaveChangesAsync();
            
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
        _logger.LogInformation("{Timestamp}: Received Arduino data from IP: {IP}.", receivedAt, ip);
        Elder? elder = await _elderManager.Users.FirstOrDefaultAsync(e => e.MacAddress == data.MacAddress);
        if (elder == null)
        {
            _logger.LogWarning("{Timestamp}: No elder found with MacAddress {MacAddress} from IP: {IP}.", receivedAt, data.MacAddress, ip);
            return;
        }

        _dbContext.GPSData.Add(new GPS
        {
            Latitude = data.Latitude,
            Longitude = data.Longitude,
            Timestamp = receivedAt,
            MacAddress = data.MacAddress
        });
        
        _dbContext.Steps.Add(new Steps()
        {
            StepsCount = data.steps,
            Timestamp = receivedAt,
            MacAddress = data.MacAddress
        });

        int totalHr = 0;
        float totalSpO2 = 0;
        foreach (var entry in data.Max30102)
        {
           totalHr += entry.heartRate;
           totalSpO2 += entry.SpO2;
        }
        if (data.Max30102.Count == 0)
        {
            _logger.LogWarning("{Timestamp}: No Max30102 data found for MacAddress {MacAddress} from IP: {IP}.", receivedAt, data.MacAddress, ip);
            return;
        }
        _dbContext.MAX30102Data.Add(new Max30102
        {
            Heartrate = totalHr / data.Max30102.Count,
            SpO2 = totalSpO2 / data.Max30102.Count,
            Timestamp = receivedAt,
            MacAddress = data.MacAddress
        });
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
