using HealthDevice.Data;
using HealthDevice.DTO;
using Microsoft.AspNetCore.Mvc;

namespace HealthDevice.Services;

public class HealthService
{
    private readonly ILogger<HealthService> _logger;
    private readonly ApplicationDbContext _db;
    
    public HealthService(ILogger<HealthService> logger, ApplicationDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task<Heartrate> CalculateHeartRate(DateTime currentDate, Elder elder)
    {
        DateTime earlierDate = currentDate.Date - TimeSpan.FromHours(1);
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
        DateTime earlierDate = currentDate.Date - TimeSpan.FromHours(1);
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
    
    
}