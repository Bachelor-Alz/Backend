using HealthDevice.DTO;
using HealthDevice.Models;
using Microsoft.EntityFrameworkCore;

// ReSharper disable SuggestVarOrType_SimpleTypes

namespace HealthDevice.Services;

public class GetHealthDataService : IGetHealthData
{
    private readonly IRepositoryFactory _repositoryFactory;
    private readonly ILogger<GetHealthDataService> _logger;
    private readonly ITimeZoneService _timeZoneService;
    private readonly IRepository<Elder> _elderRepository;

    public GetHealthDataService(IRepositoryFactory repositoryFactory, ILogger<GetHealthDataService> logger,
        ITimeZoneService timeZoneService, IRepository<Elder> elderRepository)
    {
        _repositoryFactory = repositoryFactory;
        _logger = logger;
        _timeZoneService = timeZoneService;
        _elderRepository = elderRepository;
    }

    public async Task<List<T>> GetHealthData<T>(string elderId, Period period, DateTime date, TimeZoneInfo timezone)
        where T : Sensor
    {
        DateTime earlierDate = PeriodUtil.GetEarlierDate(date, period);
        earlierDate = _timeZoneService.LocalTimeToUTC(timezone, earlierDate);
        date = _timeZoneService.LocalTimeToUTC(timezone, date);
        _logger.LogInformation("Fetching data for period: {Period}, Date Range: {EarlierDate} to {Date}", period,
            earlierDate, date);

        Elder? elder = await _elderRepository.Query()
            .FirstOrDefaultAsync(e => e.Id == elderId);

        if (elder == null || string.IsNullOrEmpty(elder.MacAddress))
        {
            _logger.LogError("No elder found or Arduino is not set");
            return [];
        }

        IRepository<T> repository = _repositoryFactory.GetRepository<T>();

        List<T> data = await repository.Query()
            .Where(d => d.MacAddress == elder.MacAddress &&
                        d.Timestamp >= earlierDate &&
                        d.Timestamp <= date)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} records for type {Type}", data.Count, typeof(T).Name);
        return data;
    }
}