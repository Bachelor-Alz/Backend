using HealthDevice.Data;
using HealthDevice.DTO;
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
        Period.Day => date - TimeSpan.FromDays(1),
        Period.Week => date - TimeSpan.FromDays(7),
        _ => throw new ArgumentException("Invalid period specified")
    };

    public Task<Heartrate> CalculateHeartRate(DateTime currentDate, string Address)
    {
        List<Max30102> heartRates = _db.MAX30102Data
            .Where(c => c.Timestamp <= currentDate && c.Address == Address)
            .ToList();
        if (heartRates.Count == 0)
        {
            _logger.LogWarning("No heart rate data found for elder");
            return Task.FromResult(new Heartrate());
        }

        IEnumerable<int> values = heartRates.Select(h => h.Heartrate);

        IEnumerable<int> enumerable = values.ToList();
        return Task.FromResult(new Heartrate
        {
            Avgrate = (int)enumerable.Average(),
            Maxrate = enumerable.Max(),
            Minrate = enumerable.Min(),
            Timestamp = currentDate
        });
    }

    public Task<Heartrate> CalculateHeartRateFromUnproccessed(List<currentHeartRate> heartRates)
    {
        if (heartRates.Count == 0)
        {
            _logger.LogWarning("No heart rate data found");
            return Task.FromResult(new Heartrate());
        }

        IEnumerable<int> values = heartRates.Select(h => h.Heartrate);

        IEnumerable<int> enumerable = values.ToList();
        return Task.FromResult(new Heartrate
        {
            Avgrate = (int)enumerable.Average(),
            Maxrate = enumerable.Max(),
            Minrate = enumerable.Min(),
            Timestamp = DateTime.UtcNow
        });
    }

    public Task<Spo2> CalculateSpo2FromUnprocessed(List<currentSpo2> spo2Data)
    {
        if (spo2Data.Count == 0)
        {
            _logger.LogWarning("No SpO2 data found");
            return Task.FromResult(new Spo2());
        }

        IEnumerable<float> values = spo2Data.Select(s => s.SpO2);

        IEnumerable<float> enumerable = values.ToList();
        return Task.FromResult(new Spo2
        {
            MinSpO2 = enumerable.Min(),
            MaxSpO2 = enumerable.Max(),
            SpO2 = enumerable.Average(),
            Timestamp = DateTime.UtcNow
        });
    }

    public Task<Spo2> CalculateSpo2(DateTime currentDate, string Arduino)
    {
        List<Max30102> spo2List = _db.MAX30102Data
            .Where(c => c.Timestamp <= currentDate && c.Address == Arduino)
            .ToList();
        if (spo2List.Count == 0)
        {
            _logger.LogWarning("No SpO2 data found for elder");
            return Task.FromResult(new Spo2());
        }

        IEnumerable<float> values = spo2List.Select(s => s.SpO2);

        IEnumerable<float> enumerable = values.ToList();
        return Task.FromResult(new Spo2
        {
            MinSpO2 = enumerable.Min(),
            MaxSpO2 = enumerable.Max(),
            SpO2 = enumerable.Average(),
            Timestamp = currentDate
        });
    }

    public Task<Kilometer> CalculateDistanceWalked(DateTime currentDate, string Arduino)
    {
        List<GPS> gpsData = _db.GPSData
            .Where(c => c.Timestamp <= currentDate && c.Address == Arduino)
            .ToList();
        if (gpsData.Count < 2)
        {
            _logger.LogWarning("Not enough GPS data to calculate distance");
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
            d += newestKilometer.Distance;
        }
        else
        {
            d += 0;
        }
        return Task.FromResult(new Kilometer
        {
            Distance = d,
            Timestamp = currentDate
        });
    }

    public async Task<ActionResult<List<T>>> GetHealthData<T>(
    string elderEmail, Period period, DateTime date, Func<T, bool> filter) where T : class
{
    DateTime earlierDate = GetEarlierDate(date, period).ToUniversalTime();
    Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
    if (elder == null || string.IsNullOrEmpty(elder.Arduino))
    {
        _logger.LogError("No elder found with email {Email} or Arduino is not set", elderEmail);
        return new BadRequestResult();
    }

    string arduino = elder.Arduino;
    IQueryable<T>? query = null;

    // Determine the correct DbSet to query based on the type T
    switch (typeof(T).Name)
    {
        case nameof(Max30102):
            query = _db.MAX30102Data
                .Where(d => d.Address == arduino && d.Timestamp >= earlierDate && d.Timestamp <= date)
                .Cast<T>();
            break;
        case nameof(GPS):
            query = _db.GPSData
                .Where(d => d.Address == arduino && d.Timestamp >= earlierDate && d.Timestamp <= date)
                .Cast<T>();
            break;
        case nameof(FallInfo):
            query = _db.FallInfo
                .Where(d => d.MacAddress == arduino && d.Timestamp >= earlierDate && d.Timestamp <= date)
                .Cast<T>();
            break;
        case nameof(Kilometer):
            query = _db.Distance
                .Where(d => d.MacAddress == arduino && d.Timestamp >= earlierDate && d.Timestamp <= date)
                .Cast<T>();
            break;
        case nameof(Steps):
            query = _db.Steps
                .Where(d => d.MacAddress == arduino && d.Timestamp >= earlierDate && d.Timestamp <= date)
                .Cast<T>();
            break;
        default:
            _logger.LogError("Unsupported type {Type}", typeof(T).Name);
            return new BadRequestResult();
    }

    if (query == null)
    {
        return new BadRequestResult();
    }

    List<T> data = query.Where(filter).ToList();
    if (data.Count != 0) return new OkObjectResult(data);
    _logger.LogWarning("No data found for elder {Email} and type {Type}", elderEmail, typeof(T).Name);
    return new BadRequestResult();

}

    public Task DeleteMax30102Data(DateTime currentDate, string Arduino)
    {
        List<Max30102> data = _db.MAX30102Data
            .Where(c => c.Timestamp <= currentDate && c.Address == Arduino)
            .ToList();
        _db.MAX30102Data.RemoveRange(data);
        return Task.CompletedTask;
    }
    public Task DeleteGpsData(DateTime currentDate, string Arduino)
    {
        List<GPS> data = _db.GPSData
            .Where(c => c.Timestamp <= currentDate && c.Address == Arduino)
            .ToList();
        _db.GPSData.RemoveRange(data);
        return Task.CompletedTask;
    }

    public async Task<ActionResult<List<T>>> GetCurrentHealthData<T>(
        string elderEmail, Period period, DateTime date, Func<Max30102, T> selector) where T : currentData
    {
        DateTime earlierDate = GetEarlierDate(date, period).ToUniversalTime();
        Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
        if (elder == null)
        {
            _logger.LogError("No elder found with email {Email}", elderEmail);
            return new BadRequestResult();
        }

        List<Max30102> data = _db.MAX30102Data
            .Where(d => d.Timestamp >= earlierDate && d.Timestamp <= date && d.Address == elder.Arduino)
            .ToList();

        if (data.Count == 0)
        {
            return new BadRequestResult();
        }
        List<T> result = data.Select(selector).ToList();

        return result;
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

        double distance = Math.Sqrt(Math.Pow((double)(location.Latitude - perimeter.Latitude), 2) + Math.Pow((double)(location.Longitude - perimeter.Longitude), 2));
        if (distance > perimeter.Radius)
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