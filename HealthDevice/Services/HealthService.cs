using HealthDevice.Data;
using HealthDevice.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HealthDevice.Services;

public class HealthService : IHealthService
{
    private readonly ILogger<HealthService> _logger;
    private readonly UserManager<Caregiver> _caregiverManager;
    private readonly EmailService _emailService;
    private readonly GeoService _geoService;
    private readonly ApplicationDbContext _db;
    private readonly UserManager<Elder> _elderManager;
    private readonly IRepositoryFactory _repositoryFactory;
    public HealthService(ILogger<HealthService> logger, UserManager<Caregiver> caregiverManager, EmailService emailService, GeoService geoService, ApplicationDbContext db, UserManager<Elder> elderManager, IRepositoryFactory repositoryFactory)
    {
        _logger = logger;
        _caregiverManager = caregiverManager;
        _emailService = emailService;
        _geoService = geoService;
        _db = db;
        _elderManager = elderManager;
        _repositoryFactory = repositoryFactory;
    }

    private DateTime GetEarlierDate(DateTime date, Period period) => period switch
    {
        Period.Hour => date - TimeSpan.FromHours(1),
        Period.Day => date.AddDays(-1),
        Period.Week => date - TimeSpan.FromDays(7),
        _ => throw new ArgumentException("Invalid period specified")
    };

    public async Task<List<Heartrate>> CalculateHeartRate(DateTime currentDate, string address)
    {
        IRepository<Max30102> repository = _repositoryFactory.GetRepository<Max30102>();
        List<Max30102> heartRates = await repository.Query()
            .Where(c => c.Timestamp <= currentDate && c.Address == address)
            .ToListAsync();

        if (heartRates.Count == 0)
        {
            _logger.LogWarning("No heart rate data found for elder {Address}", address);
            return new List<Heartrate>();
        }

        DateTime earliestDate = heartRates.Min(h => h.Timestamp);
        List<Heartrate> heartRateList = new();

        for (DateTime date = earliestDate; date <= currentDate; date = date.AddHours(1))
        {
            var date1 = date;
            var heartRateInHour = heartRates.Where(h => h.Timestamp >= date1 && h.Timestamp < date1.AddHours(1));
            List<Max30102> rateInHour = heartRateInHour.ToList();
            if (rateInHour.Count == 0) continue;

            _logger.LogInformation("Heart rate data found for mac-address {Address} in hour {Hour}", address, date);
            heartRateList.Add(new Heartrate
            {
                Avgrate = (int)rateInHour.Average(h => h.Heartrate),
                Maxrate = rateInHour.Max(h => h.Heartrate),
                Minrate = rateInHour.Min(h => h.Heartrate),
                Timestamp = date
            });
        }

        return heartRateList;
    }

    public async Task<List<Spo2>> CalculateSpo2(DateTime currentDate, string address)
    {
        IRepository<Max30102> repository = _repositoryFactory.GetRepository<Max30102>();
        List<Max30102> spo2Data = await repository.Query()
            .Where(c => c.Timestamp <= currentDate && c.Address == address)
            .ToListAsync();

        if (spo2Data.Count == 0)
        {
            _logger.LogWarning("No SpO2 data found for elder {Address}", address);
            return new List<Spo2>();
        }

        DateTime earliestDate = spo2Data.Min(s => s.Timestamp);
        List<Spo2> spo2List = new();

        for (DateTime date = earliestDate; date <= currentDate; date = date.AddHours(1))
        {
            var hourlyData = spo2Data.Where(s => s.Timestamp >= date && s.Timestamp < date.AddHours(1)).ToList();
            if (hourlyData.Count == 0) continue;

            _logger.LogInformation("SpO2 data found for mac-address {Address} in hour {Hour}", address, date);
            spo2List.Add(new Spo2
            {
                AvgSpO2 = hourlyData.Average(s => s.SpO2),
                MaxSpO2 = hourlyData.Max(s => s.SpO2),
                MinSpO2 = hourlyData.Min(s => s.SpO2),
                Timestamp = date
            });
        }

        return spo2List;
    }

    public async Task<Kilometer> CalculateDistanceWalked(DateTime currentDate, string arduino)
    {
        IRepository<GPS> repository = _repositoryFactory.GetRepository<GPS>();
        List<GPS> gpsData = await repository.Query()
            .Where(c => c.Timestamp.Date <= currentDate.Date && c.Address == arduino)
            .ToListAsync();

        if (gpsData.Count < 2)
        {
            _logger.LogWarning("Not enough GPS data to calculate distance for elder {Arduino}", arduino);
            return new Kilometer();
        }

        double distance = 0;
        for (int i = 0; i < gpsData.Count - 1; i++)
        {
            double a = Math.Pow(Math.Sin((gpsData[i + 1].Latitude - gpsData[i].Latitude) / 2), 2) +
                       Math.Cos(gpsData[i].Latitude) * Math.Cos(gpsData[i + 1].Latitude) *
                       Math.Pow(Math.Sin((gpsData[i + 1].Longitude - gpsData[i].Longitude) / 2), 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            distance += 6371 * c; // Earth's radius in kilometers
        }

        if (distance == 0)
        {
            _logger.LogWarning("No distance data found for elder {Arduino}", arduino);
            return new Kilometer();
        }

        _logger.LogInformation("Distance data found for elder {Arduino}", arduino);
        return new Kilometer
        {
            Distance = distance,
            Timestamp = currentDate
        };
    }
    public async Task<List<T>> GetHealthData<T>(string elderEmail, Period period, DateTime date) where T : class
    {
        DateTime earlierDate = GetEarlierDate(date, period).ToUniversalTime();
        IRepository<Elder> elderRepository = _repositoryFactory.GetRepository<Elder>();

        Elder? elder = await elderRepository.Query()
            .FirstOrDefaultAsync(e => e.Email == elderEmail);
        if (elder == null || string.IsNullOrEmpty(elder.Arduino))
        {
            _logger.LogError("No elder found with email {Email} or Arduino is not set", elderEmail);
            return new List<T>();
        }

        string arduino = elder.Arduino;
        IRepository<T> repository = _repositoryFactory.GetRepository<T>();

        List<T> data = await repository.Query()
            .Where(d => EF.Property<string>(d, "MacAddress") == arduino &&
                        EF.Property<DateTime>(d, "Timestamp") >= earlierDate &&
                        EF.Property<DateTime>(d, "Timestamp") <= date)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} records for type {Type}", data.Count, typeof(T).Name);
        return data;
    }

   public async Task DeleteMax30102Data(DateTime currentDate, string arduino)
{
    IRepository<Max30102> repository = _repositoryFactory.GetRepository<Max30102>();
    List<Max30102> data = await repository.Query()
        .Where(c => c.Timestamp <= currentDate && c.Address == arduino)
        .ToListAsync();

    if (data.Count == 0)
    {
        _logger.LogWarning("No Max30102 data found to delete for elder {Arduino}", arduino);
        return;
    }

    repository.RemoveRange(data);
    _logger.LogInformation("Deleted {Count} Max30102 records for elder {Arduino}", data.Count, arduino);
}
    public async Task DeleteGpsData(DateTime currentDate, string arduino)
    {
        IRepository<GPS> repository = _repositoryFactory.GetRepository<GPS>();
        List<GPS> data = await repository.Query()
            .Where(c => c.Timestamp <= currentDate && c.Address == arduino)
            .ToListAsync();

        if (data.Count == 0)
        {
            _logger.LogWarning("No GPS data found to delete for elder {Arduino}", arduino);
            return;
        }

        repository.RemoveRange(data);
        _logger.LogInformation("Deleted {Count} GPS records for elder {Arduino}", data.Count, arduino);
    }

    public async Task ComputeOutOfPerimeter(string Arduino, Location location)
    {
        IRepository<Perimeter> perimeterRepository = _repositoryFactory.GetRepository<Perimeter>();
        IRepository<Elder> elderRepository = _repositoryFactory.GetRepository<Elder>();

        Perimeter? perimeter = await perimeterRepository.Query()
            .FirstOrDefaultAsync(p => p.MacAddress == Arduino);
        if (perimeter == null)
        {
            _logger.LogWarning("No perimeter found for elder with Arduino {Arduino}", Arduino);
            return;
        }

        Elder? elder = await elderRepository.Query()
            .FirstOrDefaultAsync(e => e.Arduino == Arduino);
        if (elder == null)
        {
            _logger.LogWarning("Elder with Arduino {Arduino} not found", Arduino);
            return;
        }
        if (perimeter.Latitude == null || perimeter.Longitude == null) return;
        double dLat = (perimeter.Latitude.Value - location.Latitude) * Math.PI / 180;
        double dLon = (perimeter.Longitude.Value - location.Longitude) * Math.PI / 180;
        double lat1 = location.Latitude * Math.PI / 180;
        double lat2 = perimeter.Latitude.Value * Math.PI / 180;

        double a = Math.Pow(Math.Sin(dLat / 2), 2) +
                   Math.Cos(lat1) * Math.Cos(lat2) *
                   Math.Pow(Math.Sin(dLon / 2), 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        double d = 6371 * c;

        Console.WriteLine($"Location: ({location.Latitude}, {location.Longitude})");
        Console.WriteLine($"Perimeter: ({perimeter.Latitude.Value}, {perimeter.Longitude.Value})");

        _logger.LogInformation("Distance from perimeter: {Distance}", d);
        if (elder.outOfPerimeter)
        {
            if (d < perimeter.Radius)
            {
                elder.outOfPerimeter = false;
                elderRepository.Update(elder);
                await _elderManager.UpdateAsync(elder);
                _logger.LogInformation("Elder {Email} is back in perimeter", elder.Email);
                return;
            }
            _logger.LogInformation("Elder {Email} is already out of perimeter", elder.Email);
            return;
        }
        if (d > perimeter.Radius)
        {
            _logger.LogInformation("Elder {Email} is out of perimeter", elder.Email);
            elder.outOfPerimeter = true;
            elderRepository.Update(elder);
            await _elderManager.UpdateAsync(elder);
        }
    }

    public async Task<Location> GetLocation(DateTime currentTime, string arduino)
    {
        IRepository<GPS> repository = _repositoryFactory.GetRepository<GPS>();
        GPS? gpsData = await repository.Query()
            .Where(c => c.Timestamp <= currentTime && c.Address == arduino)
            .OrderByDescending(c => c.Timestamp)
            .FirstOrDefaultAsync();

        if (gpsData != null)
        {
            _logger.LogInformation("GPS data found for elder {Arduino} at {Time}", arduino, currentTime);
            return new Location
            {
                Latitude = gpsData.Latitude,
                Longitude = gpsData.Longitude,
                Timestamp = gpsData.Timestamp
            };
        }

        _logger.LogWarning("No GPS data found for elder {Arduino}", arduino);
        return new Location();
    }
}