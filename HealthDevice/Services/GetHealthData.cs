using HealthDevice.DTO;
using HealthDevice.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthDevice.Services;

public class GetHealthDataService : IGetHealthData
{
    private readonly IRepositoryFactory _repositoryFactory;
    private readonly ILogger<GetHealthDataService> _logger;
    private readonly ITimeZoneService _timeZoneService;
    
    public GetHealthDataService(IRepositoryFactory repositoryFactory, ILogger<GetHealthDataService> logger, ITimeZoneService timeZoneService)
    {
        _repositoryFactory = repositoryFactory;
        _logger = logger;
        _timeZoneService = timeZoneService;
    }
    
    private DateTime GetEarlierDate(DateTime date, Period period) => period switch
    {
        Period.Hour => date - TimeSpan.FromHours(1),
        Period.Day => date.Date,
        Period.Week => date.AddDays(-6).Date,
        _ => throw new ArgumentException("Invalid period specified")
    };
    
    public async Task<List<T>> GetHealthData<T>(string elderEmail, Period period, DateTime date, TimeZoneInfo timezone) where T : class
    {
        DateTime earlierDate = GetEarlierDate(date, period);
        earlierDate = _timeZoneService.GetCurrentTimeIntLocalTime(timezone, earlierDate);
        date = _timeZoneService.GetCurrentTimeIntLocalTime(timezone, date);
        
        _logger.LogInformation("Fetching data for period: {Period}, Date Range: {EarlierDate} to {Date}", period, earlierDate, date);
    
        IRepository<Elder> elderRepository = _repositoryFactory.GetRepository<Elder>();
        Elder? elder = await elderRepository.Query()
            .FirstOrDefaultAsync(e => e.Email == elderEmail);

        if (elder == null || string.IsNullOrEmpty(elder.MacAddress))
        {
            _logger.LogError("No elder found with email {Email} or Arduino is not set", elderEmail);
            return new List<T>();
        }

        string arduino = elder.MacAddress;
        IRepository<T> repository = _repositoryFactory.GetRepository<T>();

        List<T> data = await repository.Query()
            .Where(d => EF.Property<string>(d, "MacAddress") == arduino &&
                        EF.Property<DateTime>(d, "Timestamp") >= earlierDate &&
                        EF.Property<DateTime>(d, "Timestamp") <= date)
            .ToListAsync();
        
        
        _logger.LogInformation("Retrieved {Count} records for type {Type}", data.Count, typeof(T).Name);
        
        
        foreach (var item in data)
        {
            var timestampProperty = typeof(T).GetProperty("Timestamp");
            if (timestampProperty != null)
            {
                DateTime utcDateTime = (DateTime)timestampProperty.GetValue(item);
                DateTimeOffset localDateTime = _timeZoneService.GetCurrentTimeInUserTimeZone(timezone, utcDateTime);
                timestampProperty.SetValue(item, localDateTime);
            }
        }
        
        return data;
    }

}