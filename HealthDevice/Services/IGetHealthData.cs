using HealthDevice.DTO;

namespace HealthDevice.Services;

public interface IGetHealthData
{
    Task<List<T>> GetHealthData<T>(string elderEmail, Period period, DateTime date, TimeZoneInfo timezone) where T : class;
}