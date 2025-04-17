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
    public HealthService(ILogger<HealthService> logger, UserManager<Caregiver> caregiverManager, EmailService emailService, GeoService geoService, ApplicationDbContext db)
    {
        _logger = logger;
        _caregiverManager = caregiverManager;
        _emailService = emailService;
        _geoService = geoService;
        _db = db;
    }

    private DateTime GetEarlierDate(DateTime date, Period period) => period switch
    {
        Period.Hour => date - TimeSpan.FromHours(1),
        Period.Day => date - TimeSpan.FromDays(1),
        Period.Week => date - TimeSpan.FromDays(7),
        _ => throw new ArgumentException("Invalid period specified")
    };

    public Task<Heartrate> CalculateHeartRate(DateTime currentDate, Elder elder)
    {
        if (elder.MAX30102Data == null || elder.MAX30102Data.Count == 0)
        {
            _logger.LogWarning("No heart rate data found for elder {Email}", elder.Email);
            return Task.FromResult(new Heartrate());
        }

        List<Max30102> heartRates = elder.MAX30102Data.Where(c => c.Timestamp <= currentDate).ToList();
        if (heartRates.Count == 0)
        {
            _logger.LogWarning("No heart rate data found for elder {Email}", elder.Email);
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

    public Task<Spo2> CalculateSpo2(DateTime currentDate, Elder elder)
    {
        if (elder.MAX30102Data == null || elder.MAX30102Data.Count == 0)
        {
            _logger.LogWarning("No SpO2 data found for elder {Email}", elder.Email);
            return Task.FromResult(new Spo2());
        }

        List<Max30102> spo2List = elder.MAX30102Data.Where(c => c.Timestamp <= currentDate).ToList();
        if (spo2List.Count == 0)
        {
            _logger.LogWarning("No SpO2 data found for elder {Email}", elder.Email);
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

    public Task<Kilometer> CalculateDistanceWalked(DateTime currentDate, Elder elder)
    {
        if (elder.GPSData == null || elder.GPSData.Count == 0)
        {
            _logger.LogWarning("No GPS data found for elder {Email}", elder.Email);
            return Task.FromResult(new Kilometer());
        }

        List<GPS> gpsData = elder.GPSData.Where(c => c.Timestamp <= currentDate).ToList();
        if (gpsData.Count < 2)
        {
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

        return Task.FromResult(new Kilometer
        {
            Distance = d,
            Timestamp = currentDate
        });
    }

    public async Task<ActionResult<List<T>>> GetHealthData<T>(string elderEmail, Period period, DateTime date, Func<Elder, List<T>?> selector, UserManager<Elder> elderManager) where T : class
    {
        DateTime earlierDate = GetEarlierDate(date, period);
        Elder? elder = await elderManager.FindByEmailAsync(elderEmail);
        if (elder == null)
        {
            _logger.LogError("No elder found with email {Email}", elderEmail);
            return new BadRequestResult();
        }

        List<T>? data = selector(elder)?.Where(d => ((dynamic)d).Timestamp >= earlierDate && ((dynamic)d).Timestamp <= date).ToList();
        if (data == null || data.Count == 0)
        {
            _logger.LogWarning("No data found for elder {Email}", elder.Email);
            return new BadRequestResult();
        }

        return new OkObjectResult(data);
    }

    public Task DeleteMax30102Data(DateTime currentDate, Elder elder)
    {
        elder.MAX30102Data?.RemoveAll(c => c.Timestamp <= currentDate);
        return Task.CompletedTask;
    }

    public Task DeleteGpsData(DateTime currentDate, Elder elder)
    {
        elder.GPSData?.RemoveAll(c => c.Timestamp <= currentDate);
        return Task.CompletedTask;
    }

    public async Task<ActionResult<List<T>>> GetCurrentHealthData<T>(
        string elderEmail, Period period, DateTime date, Func<Max30102, T> selector, UserManager<Elder> elderManager) where T : currentData
    {
        DateTime earlierDate = GetEarlierDate(date, period);
        Elder? elder = await elderManager.FindByEmailAsync(elderEmail);
        if (elder == null)
        {
            _logger.LogError("No elder found with email {Email}", elderEmail);
            return new BadRequestResult();
        }

        List<Max30102> data = _db.MAX30102Data
            .Where(d => d.Timestamp >= earlierDate && d.Timestamp <= date && d.Address == elder.Arduino)
            .ToList();

        if (!data.Any())
        {
            return new BadRequestResult();
        }
        List<T> result = data.Select(selector).ToList();

        return result;
    }

    public async Task ComputeOutOfPerimeter(Elder elder)
    {
        if (elder.Perimeter?.Latitude == null || elder.Perimeter?.Longitude == null || elder.Location == null) return;

        double distance = Math.Sqrt(Math.Pow((double)(elder.Location.Latitude - elder.Perimeter.Latitude), 2) + Math.Pow((double)(elder.Location.Longitude - elder.Perimeter.Longitude), 2));
        if (distance > elder.Perimeter.Radius)
        {
            List<Caregiver> caregivers = _caregiverManager.Users.Where(c => c.Elders != null && c.Elders.Contains(elder)).ToList();
            if (caregivers.Count == 0)
            {
                _logger.LogWarning("No caregivers found for elder {Email}", elder.Email);
                return;
            }

            string address = await _geoService.GetAddressFromCoordinates(elder.Location.Latitude, elder.Location.Longitude);

            foreach (var caregiver in caregivers.Where(c => c.Email != null))
            {
                var emailInfo = new Email { name = caregiver.Name, email = caregiver.Email };
                _logger.LogInformation("Sending email to {CaregiverEmail}", caregiver.Email);
                await _emailService.SendEmail(emailInfo, "Elder out of perimeter", $"Elder {elder.Name} is out of perimeter, at location {address}.");
            }
        }
    }

    public Task<Location> GetLocation(DateTime currentTime, Elder elder)
    {
        var gps = elder.GPSData?.FirstOrDefault(g => g.Timestamp <= currentTime);
        if (gps != null)
        {
            return Task.FromResult(new Location
            {
                Latitude = gps.Latitude,
                Longitude = gps.Longitude,
                Timestamp = gps.Timestamp
            });
        }

        _logger.LogWarning("No GPS data found for elder {Email}", elder.Email);
        return Task.FromResult(new Location());
    }
}