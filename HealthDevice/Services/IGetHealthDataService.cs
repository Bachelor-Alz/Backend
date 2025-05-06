using HealthDevice.DTO;

namespace HealthDevice.Services;

public interface IGetHealthDataService
{
    Task<List<T>> GetHealthData<T>(string elderEmail, Period period, DateTime date) where T : class;
}