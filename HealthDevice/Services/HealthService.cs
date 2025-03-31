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
    public async Task<Heartrate> CalculateHeartRate(DateTime currentDate, Elder elder)
    {
        List<Max30102> heartRates = elder.Max30102Data.Where(c => c.Timestamp <= currentDate).ToList();
        List<int> heartRateValues = heartRates.Select(h => h.HeartRate).ToList();

        return new Heartrate
        {
            AvgRate = (int)heartRateValues.Average(),
            MaxRate = heartRateValues.Max(),
            MinRate = heartRateValues.Min(),
            Timestamp = currentDate,
            Id = -1
        };
    }
    
    public async Task<Spo2> CalculateSpo2(DateTime currentDate, Elder elder)
    {
        List<Max30102> Spo2s = elder.Max30102Data.Where(c => c.Timestamp <= currentDate).ToList();
        List<float> Spo2Values = Spo2s.Select(s => s.SpO2).ToList();

        return new Spo2
        {
            Id = -1,
            MinSpO2 = Spo2Values.Min(),
            MaxSpO2 = Spo2Values.Max(),
            spO2 = Spo2Values.Average(),
            Timestamp = currentDate,
        };
    }
    public async Task<Kilometer> CalculateDistanceWalked(DateTime currentDate, Elder elder)
    {
        List<GPS> gpsData = elder.GpsData.Where(c => c.Timestamp <= currentDate).ToList();

        //Math for distance calculation
        double d = 0;
        for(int i = 0; i < gpsData.Count - 1; i++)
        {
            double a = Math.Pow(Math.Sin((gpsData[i + 1].Latitude - gpsData[i].Latitude) / 2), 2) + Math.Cos(gpsData[i].Latitude) * Math.Cos(gpsData[i + 1].Latitude) * Math.Pow(Math.Sin((gpsData[i + 1].Longitude - gpsData[i].Longitude) / 2), 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            d += 6371 * c;
        }
        
        return new Kilometer
        {
            distance = d,
            timestamp = currentDate
        };
    }
    
    public async Task<ActionResult<List<T>>> GetHealthData<T>(string elderEmail, Period period, DateTime date, Func<Elder, List<T>> selector, UserManager<Elder> _elderManager) where T : class
    {
        DateTime earlierDate = period switch
        {
            Period.Hour => date - TimeSpan.FromHours(1),
            Period.Day => date - TimeSpan.FromDays(1),
            Period.Week => date - TimeSpan.FromDays(7),
            _ => throw new ArgumentException("Invalid period specified")
        };

        Elder? elder = await _elderManager.FindByEmailAsync(elderEmail);
        if (elder == null)
        {
            _logger.LogError("No elder found with email {email}", elderEmail);
            return new BadRequestResult();
        }

        List<T> data = selector(elder).Where(d => ((dynamic)d).Timestamp >= earlierDate && ((dynamic)d).Timestamp <= date).ToList();
        return data;
    }
    
    public async Task DeleteMax30102Data(DateTime currentDate, Elder elder)
    {
        List<Max30102> max30102s = elder.Max30102Data.Where(c => c.Timestamp <= currentDate).ToList();
        
        foreach (Max30102 max30102 in max30102s)
        {
            elder.Max30102Data.Remove(max30102);
        }
    }
    
    public async Task DeleteGPSData(DateTime currentDate, Elder elder)
    {
        List<GPS> gpsData = elder.GpsData.Where(c => c.Timestamp <= currentDate).ToList();
        
        foreach (GPS gps in gpsData)
        {
            elder.GpsData.Remove(gps);
        }
    }
}