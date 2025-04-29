using HealthDevice.DTO;
using Microsoft.EntityFrameworkCore;

namespace HealthDevice.Services;

public class GetHealthDataService : IGetHealthData
{
    private readonly IRepositoryFactory _repositoryFactory;
    private readonly ILogger<GetHealthDataService> _logger;
    
    public GetHealthDataService(IRepositoryFactory repositoryFactory, ILogger<GetHealthDataService> logger)
    {
        _repositoryFactory = repositoryFactory;
        _logger = logger;
    }
    
    private DateTime GetEarlierDate(DateTime date, Period period) => period switch
    {
        Period.Hour => date - TimeSpan.FromHours(1),
        Period.Day => date.Date,
        Period.Week => date.AddDays(-6).Date,
        _ => throw new ArgumentException("Invalid period specified")
    };
    
    public async Task<List<T>> GetHealthData<T>(string elderEmail, Period period, DateTime date) where T : class
    {
        DateTime earlierDate = GetEarlierDate(date, period).ToUniversalTime();
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
        return data;
    }
}