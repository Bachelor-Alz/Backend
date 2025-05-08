using HealthDevice.DTO;
using HealthDevice.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthDevice.Services;

public class GetHealthDataService : IGetHealthData
{
    private readonly IRepositoryFactory _repositoryFactory;
    private readonly ILogger<GetHealthDataService> _logger;
    private readonly ITimeZoneService _timeZoneService;
    private readonly IRepository<Elder> _elderRepository;

    public GetHealthDataService(IRepositoryFactory repositoryFactory, ILogger<GetHealthDataService> logger, ITimeZoneService timeZoneService, IRepository<Elder> elderRepository)
    {
        _repositoryFactory = repositoryFactory;
        _logger = logger;
        _timeZoneService = timeZoneService;
        _elderRepository = elderRepository;
    }
    
    private DateTime GetEarlierDate(DateTime date, Period period) => period switch
    {
        Period.Hour => date - TimeSpan.FromHours(1),
        Period.Day => date.Date,
        Period.Week => date.AddDays(-6).Date,
        _ => throw new ArgumentException("Invalid period specified")
    };
    
    public async Task<List<T>> GetHealthData<T>(string elderEmail, Period period, DateTime date, TimeZoneInfo timezone) where T : Sensor
    {
        DateTime earlierDate = GetEarlierDate(date, period);
        earlierDate = _timeZoneService.LocalTimeToUTC(timezone, earlierDate);
        date = _timeZoneService.LocalTimeToUTC(timezone, date);
        _logger.LogInformation("Fetching data for period: {Period}, Date Range: {EarlierDate} to {Date}", period, earlierDate, date);
    
        IRepository<Elder> elderRepository = _elderRepository;
        Elder? elder = await elderRepository.Query()
            .FirstOrDefaultAsync(e => e.Email == elderEmail);

        if (elder == null || string.IsNullOrEmpty(elder.MacAddress))
        {
            _logger.LogError("No elder found with email {Email} or Arduino is not set", elderEmail);
            return [];
        }

        string arduino = elder.MacAddress;
        IRepository<T> repository = _repositoryFactory.GetRepository<T>();

        List<T> data = await repository.Query()
            .Where(d => d.MacAddress == arduino &&
                         d.Timestamp >= earlierDate &&
                         d.Timestamp <= date)
            .ToListAsync();
        
        
        _logger.LogInformation("Retrieved {Count} records for type {Type}", data.Count, typeof(T).Name);
        return data;
    }

}