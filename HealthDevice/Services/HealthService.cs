using HealthDevice.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthDevice.Services;

public class HealthService
{
    private readonly ILogger<HealthService> _logger;
    private readonly UserManager<Elder> _elderManager;
    
    public HealthService(ILogger<HealthService> logger, UserManager<Elder> elderManager)
    {
        _logger = logger;
        _elderManager = elderManager;
    }
    public async Task<Heartrate> CalculateHeartRate(DateTime currentDate, Elder elder)
    {
        DateTime earlierDate = currentDate - TimeSpan.FromHours(1);
        List<Max30102> heartRates = elder.Max30102Datas.Where(c => c.Timestamp >= earlierDate && c.Timestamp <= currentDate).ToList();
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
        DateTime earlierDate = currentDate - TimeSpan.FromHours(1);
        List<Max30102> Spo2s = elder.Max30102Datas.Where(c => c.Timestamp >= earlierDate && c.Timestamp <= currentDate).ToList();
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
        DateTime earlierDate = currentDate - TimeSpan.FromHours(1);
        List<GPS> gpsData = elder.gpsData.Where(c => c.Timestamp >= earlierDate && c.Timestamp <= currentDate).ToList();
        
        //Math for distance walked
        List<double> a = new List<double>();
        List<double> c = new List<double>();
        List<double> d = new List<double>();

        for(int i = 0; i < gpsData.Count - 1; i++)
        {
            a.Add(Math.Pow(Math.Sin((gpsData[i + 1].Latitude - gpsData[i].Latitude) / 2), 2) + Math.Cos(gpsData[i].Latitude) * Math.Cos(gpsData[i + 1].Latitude) * Math.Pow(Math.Sin((gpsData[i + 1].Longitude - gpsData[i].Longitude) / 2), 2));
            c.Add(2 * Math.Atan2(Math.Sqrt(a[i]), Math.Sqrt(1 - a[i])));
            d.Add(6371 * c[i]);
        }
        
        
        return new Kilometer
        {
            Id = -1,
            distance = d.Sum(),
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
        DateTime earlierDate = currentDate - TimeSpan.FromHours(1);
        List<Max30102> max30102s = elder.Max30102Datas.Where(c => c.Timestamp >= earlierDate && c.Timestamp <= currentDate).ToList();
        
        foreach (Max30102 max30102 in max30102s)
        {
            elder.Max30102Datas.Remove(max30102);
        }
        await _elderManager.UpdateAsync(elder);
    }
    
    public async Task DeleteGPSData(DateTime currentDate, Elder elder)
    {
        DateTime earlierDate = currentDate - TimeSpan.FromHours(1);
        List<GPS> gpsData = elder.gpsData.Where(c => c.Timestamp >= earlierDate && c.Timestamp <= currentDate).ToList();
        
        foreach (GPS gps in gpsData)
        {
            elder.gpsData.Remove(gps);
        }
        await _elderManager.UpdateAsync(elder);
    }
}