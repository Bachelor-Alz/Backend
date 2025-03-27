using HealthDevice.Data;
using HealthDevice.DTO;
using Microsoft.AspNetCore.Mvc;
namespace HealthDevice.Services;

public class ArduinoService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ArduinoService> _logger;

    public ArduinoService(ApplicationDbContext context, ILogger<ArduinoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ActionResult> HandleSensorData(List<IMU> data, HttpContext httpContext)
    {
        string ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        DateTime receivedAt = DateTime.UtcNow;

        if (data.Count == 0)
        {
            _logger.LogWarning("{Timestamp}: IMU data was empty from IP: {IP}.", receivedAt, ip);
            return new BadRequestObjectResult("IMU data is empty.");
        }

        foreach (var imu in data)
        {
            imu.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(imu.EpochTimestamp).UtcDateTime;
        }

        _context.Mpu6050Data.AddRange(data);
        await _context.SaveChangesAsync();

        _logger.LogInformation("{Timestamp}: Saved {Count} IMU entries from IP: {IP}.", receivedAt, data.Count, ip);
        return new OkResult();
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

        foreach (var gps in data)
        {
            gps.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(gps.EpochTimestamp).UtcDateTime;
        }

        _context.GpsData.AddRange(data);
        await _context.SaveChangesAsync();

        _logger.LogInformation("{Timestamp}: Saved {Count} GPS entries from IP: {IP}.", receivedAt, data.Count, ip);
        return new OkResult();
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

        foreach (var entry in data)
        {
            entry.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(entry.EpochTimestamp).UtcDateTime;
        }

        _context.Max30102Data.AddRange(data);
        await _context.SaveChangesAsync();

        _logger.LogInformation("{Timestamp}: Saved {Count} MAX30102 entries from IP: {IP}.", receivedAt, data.Count, ip);
        return new OkResult();
    }
}
