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
    
    public async Task<ActionResult<List<T>>> GetHealthData<T>(string elderEmail, Period period, DateTime date, Func<Elder, List<T>> selector, UserManager<Elder> _elderManager) where T : class
    {
        DateTime earlierDate = period switch
        {
            Period.Hour => date - TimeSpan.FromHours(1),
            Period.Day => date - TimeSpan.FromDays(1),
            Period.Week => date - TimeSpan.FromDays(7),
            Period.Month => date - TimeSpan.FromDays(30),
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
    
    
}