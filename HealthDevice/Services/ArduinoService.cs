using HealthDevice.DTO;
using HealthDevice.Data;
using HealthDevice.Models;
using Microsoft.AspNetCore.Mvc;

namespace HealthDevice.Services;

public class ArduinoService : IArduinoService
{
    private readonly ILogger<ArduinoService> _logger;
    private readonly IRepositoryFactory _repositoryFactory;
    private readonly ApplicationDbContext _dbContext;
    private readonly IRepository<GPSData> _gpsRepository;
    private readonly IRepository<Max30102> _max30102Repository;
    private readonly IRepository<Steps> _stepsRepository;

    public ArduinoService
    (
        ILogger<ArduinoService> logger,
        IRepositoryFactory repositoryFactory,
        ApplicationDbContext dbContext,
        IRepository<GPSData> gpsRepository,
        IRepository<Max30102> max30102Repository,
        IRepository<Steps> stepsRepository
    )
    {
        _logger = logger;
        _repositoryFactory = repositoryFactory;
        _dbContext = dbContext;
        _gpsRepository = gpsRepository;
        _max30102Repository = max30102Repository;
        _stepsRepository = stepsRepository;
    }

    public async Task<ActionResult> HandleSensorData<T>(List<T> data, HttpContext httpContext) where T : Sensor
    {
        IRepository<T> sensorRepository = _repositoryFactory.GetRepository<T>();
        string ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        DateTime receivedAt = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0).ToUniversalTime();

        if (data.Count == 0)
        {
            _logger.LogWarning("{Timestamp}: {SensorType} data was empty from IP: {IP}.", receivedAt, typeof(T).Name, ip);
            return new BadRequestObjectResult($"{typeof(T).Name} data is empty.");
        }

        _logger.LogInformation("{Timestamp}: Data found with MacAddress {MacAddress} from IP: {IP}.", receivedAt, data.First().MacAddress, ip);
        await sensorRepository.AddRange(data);
        await _dbContext.SaveChangesAsync();
        return new OkResult();
    }

    public async Task HandleArduinoData(ArduinoDTO data, HttpContext httpContext)
    {
        string ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        DateTime receivedAt = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0).ToUniversalTime();
        _logger.LogInformation("{Timestamp}: Received Arduino data from IP: {IP}.", receivedAt, ip);

        await _gpsRepository.Add(new GPSData
        {
            Latitude = data.Latitude,
            Longitude = data.Longitude,
            Timestamp = receivedAt,
            MacAddress = data.MacAddress
        });

        await _stepsRepository.Add(new Steps
        {
            StepsCount = data.Steps,
            Timestamp = receivedAt,
            MacAddress = data.MacAddress
        });

        int totalHr = 0;
        float totalSpO2 = 0;
        foreach (var entry in data.Max30102)
        {
            totalHr += entry.HeartRate;
            totalSpO2 += entry.SpO2;
        }
        if (data.Max30102.Count == 0)
        {
            _logger.LogWarning("{Timestamp}: No Max30102 data found for MacAddress {MacAddress} from IP: {IP}.", receivedAt, data.MacAddress, ip);
            return;
        }
        await _max30102Repository.Add(new Max30102
        {
            LastHeartrate = data.Max30102.Last().HeartRate,
            AvgHeartrate = totalHr / data.Max30102.Count,
            MaxHeartrate = data.Max30102.Max(x => x.HeartRate),
            MinHeartrate = data.Max30102.Min(x => x.HeartRate),
            LastSpO2 = data.Max30102.Last().SpO2,
            AvgSpO2 = totalSpO2 / data.Max30102.Count,
            MaxSpO2 = data.Max30102.Max(x => x.SpO2),
            MinSpO2 = data.Max30102.Min(x => x.SpO2),
            Timestamp = receivedAt,
            MacAddress = data.MacAddress
        });
        await _dbContext.SaveChangesAsync();
    }
}
