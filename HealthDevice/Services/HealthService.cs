using HealthDevice.Data;
using HealthDevice.DTO;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthDevice.Services;

public class HealthService
{
    private readonly ILogger<HealthService> _logger;
    private readonly UserManager<Caregiver> _caregiverManager;
    private readonly EmailService _emailService;
    private readonly GeoService _geoService;
    private readonly ApplicationDbContext _db;
    private readonly UserManager<Elder> _elderManager;
    public HealthService(ILogger<HealthService> logger, UserManager<Caregiver> caregiverManager, EmailService emailService, GeoService geoService, ApplicationDbContext db, UserManager<Elder> elderManager)
    {
        _logger = logger;
        _caregiverManager = caregiverManager;
        _emailService = emailService;
        _geoService = geoService;
        _db = db;
        _elderManager = elderManager;
    }

    private DateTime GetEarlierDate(DateTime date, Period period) => period switch
    {
        Period.Hour => date - TimeSpan.FromHours(1),
        Period.Day => date.AddDays(-1),
        Period.Week => date - TimeSpan.FromDays(7),
        _ => throw new ArgumentException("Invalid period specified")
    };

    public Task<List<Heartrate>> CalculateHeartRate(DateTime currentDate, string Address)
    {
        List<Max30102> heartRates = _db.MAX30102Data
            .Where(c => c.Timestamp <= currentDate && c.Address == Address)
            .ToList();
        if (heartRates.Count == 0)
        {
            _logger.LogWarning("No heart rate data found for elder {Address}", Address);
            return Task.FromResult(new List<Heartrate>());
        }

        IEnumerable<int> values = heartRates.Select(h => h.Heartrate);

        IEnumerable<int> enumerable = values.ToList();
        
        DateTime earliestDate = heartRates.Min(h => h.Timestamp);
        
        //What to calc for each hour
        List<Heartrate> heartRateList = new List<Heartrate>();
        for (DateTime date = earliestDate; date <= currentDate; date = date.AddHours(1))
        {
            var date1 = date;
            var heartRateInHour = heartRates.Where(h => h.Timestamp >= date1 && h.Timestamp < date1.AddHours(1));
            IEnumerable<Max30102> rateInHour = heartRateInHour.ToList();
            if (rateInHour.Any())
            {
                _logger.LogInformation("Heart rate data found for mac-address {Address} in hour {Hour}", Address, date);
                heartRateList.Add(new Heartrate
                {
                    Avgrate = (int)rateInHour.Average(h => h.Heartrate),
                    Maxrate = rateInHour.Max(h => h.Heartrate),
                    Minrate = rateInHour.Min(h => h.Heartrate),
                    Timestamp = date
                });
            }
        }
        if (heartRateList.Count == 0)
        {
            _logger.LogWarning("No heart rate data found for elder {Address}", Address);
            return Task.FromResult(new List<Heartrate>());
        }
        _logger.LogInformation("Heart rate data found for elder {Address}", Address);
        return Task.FromResult(heartRateList);
    }

    public Task<List<Spo2>> CalculateSpo2(DateTime currentDate, string address)
    {
        List<Max30102> spo2Data = _db.MAX30102Data
            .Where(c => c.Timestamp <= currentDate && c.Address == address)
            .ToList();

        if (spo2Data.Count == 0)
        {
            _logger.LogWarning("No SpO2 data found for elder {Address}", address);
            return Task.FromResult(new List<Spo2>());
        }

        DateTime earliestDate = spo2Data.Min(s => s.Timestamp);

        List<Spo2> spo2List = new List<Spo2>();
        for (DateTime date = earliestDate; date <= currentDate; date = date.AddHours(1))
        {
            var hourlyData = spo2Data.Where(s => s.Timestamp >= date && s.Timestamp < date.AddHours(1)).ToList();
            if (hourlyData.Any())
            {
                _logger.LogInformation("SpO2 data found for mac-address {Address} in hour {Hour}", address, date);
                spo2List.Add(new Spo2
                {
                    AvgSpO2 = hourlyData.Average(s => s.SpO2),
                    MaxSpO2 = hourlyData.Max(s => s.SpO2),
                    MinSpO2 = hourlyData.Min(s => s.SpO2),
                    Timestamp = date
                });
            }
        }
        if (spo2List.Count == 0)
        {
            _logger.LogWarning("No SpO2 data found for elder {Address}", address);
            return Task.FromResult(new List<Spo2>());
        }
        _logger.LogInformation("SpO2 data found for elder {Address}", address);
        return Task.FromResult(spo2List);
    }

    public Task<Kilometer> CalculateDistanceWalked(DateTime currentDate, string Arduino)
    {
        List<GPS> gpsData = _db.GPSData
            .Where(c => c.Timestamp <= currentDate && c.Address == Arduino)
            .ToList();
        if (gpsData.Count < 2)
        {
            _logger.LogWarning("Not enough GPS data to calculate distance for elder {Arduino}", Arduino);
            return Task.FromResult(new Kilometer());
        }
        
        double d = 0;
        for (int i = 0; i < gpsData.Count - 1; i++)
        {
            double a = Math.Pow(Math.Sin((gpsData[i + 1].Latitude - gpsData[i].Latitude) / 2), 2) +
                       Math.Cos(gpsData[i].Latitude) * Math.Cos(gpsData[i + 1].Latitude) *
                       Math.Pow(Math.Sin((gpsData[i + 1].Longitude - gpsData[i].Longitude) / 2), 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            d += 6371 * c;
        }
        Kilometer? newestKilometer = _db.Distance
            .Where(c => c.Timestamp <= currentDate && c.MacAddress == Arduino)
            .ToList().LastOrDefault();
        if (newestKilometer != null && newestKilometer.Timestamp.Date == currentDate.Date)
        {
            _logger.LogInformation("Found existing distance data for mac-address {Arduino} on date {Date}", Arduino, currentDate);
            d += newestKilometer.Distance;
        }
        else
        {
            _logger.LogInformation("No existing distance data found for mac-address {Arduino} on date {Date}, So creating a new", Arduino, currentDate);
            d += 0;
        }
        if (d == 0)
        {
            _logger.LogWarning("No distance data found for elder {Arduino}", Arduino);
            return Task.FromResult(new Kilometer());
        }
        _logger.LogInformation("Distance data found for elder {Arduino}", Arduino);
        return Task.FromResult(new Kilometer
        {
            Distance = d,
            Timestamp = currentDate
        });
    }

    public async Task<List<T>> GetHealthData<T>(
    string elderEmail, Period period, DateTime date, Func<T, bool> filter) where T : class
{
    DateTime earlierDate = GetEarlierDate(date, period).ToUniversalTime();
    Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
    if (elder == null || string.IsNullOrEmpty(elder.Arduino))
    {
        _logger.LogError("No elder found with email {Email} or Arduino is not set", elderEmail);
        return new List<T>();
    }

    string arduino = elder.Arduino;
    IQueryable<T> sensorData;

    // Determine the correct DbSet to query based on the type T
    switch (typeof(T).Name)
    {
        case nameof(Max30102):
            sensorData = _db.MAX30102Data
                .Where(d => d.Address == arduino && d.Timestamp >= earlierDate && d.Timestamp <= date)
                .Cast<T>();
            break;
        case nameof(GPS):
            sensorData = _db.GPSData
                .Where(d => d.Address == arduino && d.Timestamp >= earlierDate && d.Timestamp <= date)
                .Cast<T>();
            break;
        case nameof(FallInfo):
            sensorData = _db.FallInfo
                .Where(d => d.MacAddress == arduino && d.Timestamp >= earlierDate && d.Timestamp <= date)
                .Cast<T>();
            break;
        case nameof(Kilometer):
            sensorData = _db.Distance
                .Where(d => d.MacAddress == arduino && d.Timestamp >= earlierDate && d.Timestamp <= date)
                .Cast<T>();
            break;
        case nameof(Steps):
            sensorData = _db.Steps
                .Where(d => d.MacAddress == arduino && d.Timestamp >= earlierDate && d.Timestamp <= date)
                .Cast<T>();
            break;
        case nameof(Heartrate):
            sensorData = _db.Heartrate
                .Where(d => d.MacAddress == arduino && d.Timestamp >= earlierDate && d.Timestamp <= date)
                .Cast<T>();
                break;
        case nameof(Spo2):
            sensorData = _db.SpO2
                .Where(d => d.MacAddress == arduino && d.Timestamp >= earlierDate && d.Timestamp <= date)
                .Cast<T>();
                break;
        default:
            _logger.LogError("Unsupported type {Type}", typeof(T).Name);
            return new List<T>();
    }

    _logger.LogInformation("Get health data complete{SensorData}", sensorData);
    _logger.LogInformation("Querying Steps data for Arduino {Arduino} between {Start} and {End}", arduino, earlierDate, date);

    List<T> data = sensorData.ToList();

    _logger.LogInformation("Retrieved {Count} records for type {Type}", data.Count, typeof(T).Name);
    if (data.Count != 0)
    {
        _logger.LogInformation("Data found for elder {Email} and type {Type}", elderEmail, typeof(T).Name);
        return data; // Ensure this is returned
    }
    else
    {
        _logger.LogWarning("No data found for elder {Email} and type {Type}", elderEmail, typeof(T).Name);
        return new List<T>();
    }

}

    public Task DeleteMax30102Data(DateTime currentDate, string Arduino)
    {
        List<Max30102> data = _db.MAX30102Data
            .Where(c => c.Timestamp <= currentDate && c.Address == Arduino)
            .ToList();
        _db.MAX30102Data.RemoveRange(data);
        _logger.LogInformation("Deleted {Count} Max30102 records for elder {Arduino}", data.Count, Arduino);
        return Task.CompletedTask;
    }
    public Task DeleteGpsData(DateTime currentDate, string Arduino)
    {
        List<GPS> data = _db.GPSData
            .Where(c => c.Timestamp <= currentDate && c.Address == Arduino)
            .ToList();
        _db.GPSData.RemoveRange(data);
        _logger.LogInformation("Deleted {Count} GPS records for elder {Arduino}", data.Count, Arduino);
        return Task.CompletedTask;
    }

    public async Task ComputeOutOfPerimeter(string Arduino, Location location)
    {
        Perimeter? perimeter = _db.Perimeter.FirstOrDefault(p => p.MacAddress == Arduino);
        if (perimeter == null)
        {
            _logger.LogWarning("No perimeter found for elder with Arduino {Arduino}", Arduino);
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
        if (d > perimeter.Radius)
        {
            Elder? elder = _elderManager.Users.FirstOrDefault(e => e.Arduino == Arduino);
            if (elder == null)
            {
                _logger.LogWarning("Elder with Arduino {Arduino} not found", Arduino);
                return;
            }
            List<Caregiver> caregivers = _caregiverManager.Users.Where(c => c.Elders != null && c.Elders.Contains(elder)).ToList();
            if (caregivers.Count == 0)
            {
                _logger.LogWarning("No caregivers found for elder {Email}", elder.Email);
                return;
            }

            string address = await _geoService.GetAddressFromCoordinates(location.Latitude, location.Longitude);

            foreach (var caregiver in caregivers.Where(c => c.Email != null))
            {
                var emailInfo = new Email { name = caregiver.Name, email = caregiver.Email };
                _logger.LogInformation("Sending email to {CaregiverEmail}", caregiver.Email);
                await _emailService.SendEmail(emailInfo, "Elder out of perimeter", $"Elder {elder.Name} is out of perimeter, at location {address}.");
            }
        }
    }

    public Task<Location> GetLocation(DateTime currentTime, string Arduino)
    {
        GPS? gpsData = _db.GPSData
            .Where(c => c.Timestamp <= currentTime && c.Address == Arduino)
            .ToList().LastOrDefault();
        if (gpsData != null)
        {
            _logger.LogInformation("GPS data found for elder {Arduino} at {Time}", Arduino, currentTime);
            return Task.FromResult(new Location
            {
                Latitude = gpsData.Latitude,
                Longitude = gpsData.Longitude,
                Timestamp = gpsData.Timestamp
            });
        }

        _logger.LogWarning("No GPS data found for elder");
        return Task.FromResult(new Location());
    }
}