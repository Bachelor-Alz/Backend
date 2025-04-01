using HealthDevice.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthDevice.Services;

public class HealthService
{
    private readonly ILogger<HealthService> _logger;
    
    public HealthService(ILogger<HealthService> logger)
    {
        _logger = logger;
    }
    public Task<Heartrate> CalculateHeartRate(DateTime currentDate, Elder elder)
    {
        List<Max30102> heartRates = elder.MAX30102Data.Where(c => c.Timestamp <= currentDate).ToList();
        
        if(heartRates.Count == 0)
        {
            _logger.LogWarning("No heart rate data found for elder {elder}", elder.Email);
            return Task.FromResult(new Heartrate());
        }
        
        List<int> heartRateValues = heartRates.Select(h => h.Heartrate).ToList();

        return Task.FromResult(new Heartrate
        {
            Avgrate = (int)heartRateValues.Average(),
            Maxrate = heartRateValues.Max(),
            Minrate = heartRateValues.Min(),
            Timestamp = currentDate,
        });
    }
    
    public Task<Spo2> CalculateSpo2(DateTime currentDate, Elder elder)
    {
        List<Max30102> spo2S = elder.MAX30102Data.Where(c => c.Timestamp <= currentDate).ToList();
        if(spo2S.Count == 0)
        {
            _logger.LogWarning("No SpO2 data found for elder {elder}", elder.Email);
            return Task.FromResult(new Spo2());
        }
        List<float> spo2Values = spo2S.Select(s => s.SpO2).ToList();

        return Task.FromResult(new Spo2
        {
            Id = -1,
            MinSpO2 = spo2Values.Min(),
            MaxSpO2 = spo2Values.Max(),
            SpO2 = spo2Values.Average(),
            Timestamp = currentDate,
        });
    }
    public Task<Kilometer> CalculateDistanceWalked(DateTime currentDate, Elder elder)
    {
        List<GPS?> gpsData = elder.GPSData.Where(c => c.Timestamp <= currentDate).ToList();
        if(gpsData.Count == 0)
        {
            _logger.LogWarning("No GPS data found for elder {elder}", elder.Email);
            return Task.FromResult(new Kilometer());
        }

        //Math for distance calculation
        double d = 0;
        for(int i = 0; i < gpsData.Count - 1; i++)
        {
            double a = Math.Pow(Math.Sin((gpsData[i + 1].Latitude - gpsData[i].Latitude) / 2), 2) + Math.Cos(gpsData[i].Latitude) * Math.Cos(gpsData[i + 1].Latitude) * Math.Pow(Math.Sin((gpsData[i + 1].Longitude - gpsData[i].Longitude) / 2), 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            d += 6371 * c;
        }
        
        return Task.FromResult(new Kilometer
        {
            Distance = d,
            Timestamp = currentDate
        });
    }
    
    public async Task<ActionResult<List<T>>> GetHealthData<T>(string elderEmail, Period period, DateTime date, Func<Elder, List<T>> selector, UserManager<Elder> elderManager) where T : class
    {
        DateTime earlierDate = period switch
        {
            Period.Hour => date - TimeSpan.FromHours(1),
            Period.Day => date - TimeSpan.FromDays(1),
            Period.Week => date - TimeSpan.FromDays(7),
            _ => throw new ArgumentException("Invalid period specified")
        };

        Elder? elder = await elderManager.FindByEmailAsync(elderEmail);
        if (elder == null)
        {
            _logger.LogError("No elder found with email {email}", elderEmail);
            return new BadRequestResult();
        }

        List<T> data = selector(elder).Where(d => ((dynamic)d).Timestamp >= earlierDate && ((dynamic)d).Timestamp <= date).ToList();
        return data;
    }
    
    public Task DeleteMax30102Data(DateTime currentDate, Elder elder)
    {
        List<Max30102> max30102S = elder.MAX30102Data.Where(c => c.Timestamp <= currentDate).ToList();
        
        foreach (Max30102 max30102 in max30102S)
        {
            elder.MAX30102Data.Remove(max30102);
        }

        return Task.CompletedTask;
    }
    
    public Task DeleteGpsData(DateTime currentDate, Elder elder)
    {
        List<GPS?> gpsData = elder.GPSData.Where(c => c.Timestamp <= currentDate).ToList();
        
        foreach (GPS? gps in gpsData)
        {
            elder.GPSData.Remove(gps);
        }

        return Task.CompletedTask;
    }

    public Task ComputeOutOfPerimeter(Elder elder)
    {
        Perimeter? perimeter = elder.Perimeter;
        if(perimeter == null)
        {
            return Task.CompletedTask;
        }
        Location lastLocation = elder.Location;

        double distance = Math.Sqrt(Math.Pow(lastLocation.Latitude - perimeter.Location.Latitude, 2) + Math.Pow(lastLocation.Longitude - perimeter.Location.Longitude, 2));
        if (distance > perimeter.Radius)
        {
            // Elder is out of the perimeter
            // Add your logic here
        }
        return Task.CompletedTask;
    }
    
    public Task<Location> GetLocation(DateTime currentTime, Elder elder)
    {
        if (elder.GPSData == null)
        {
            _logger.LogWarning("No GPS data found for elder {elder}", elder.Email);
            return Task.FromResult(elder.Location);
        }

        GPS? gps = elder.GPSData.FirstOrDefault(g => g?.Timestamp <= currentTime);
        if (gps != null)
            return Task.FromResult(new Location
            {
                Latitude = gps.Latitude,
                Longitude = gps.Longitude,
                Timestamp = gps.Timestamp
            });
        _logger.LogWarning("No GPS data found for elder {elder}", elder.Email);
        return Task.FromResult(new Location());
    }
}