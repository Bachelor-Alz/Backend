using HealthDevice.DTO;

namespace HealthDevice.Services;

public interface IHealthService
{
    Task<List<Heartrate>> CalculateHeartRate(DateTime currentDate, string address);
    Task<List<Spo2>> CalculateSpo2(DateTime currentDate, string address);
    Task<Kilometer> CalculateDistanceWalked(DateTime currentDate, string arduino);
    Task<List<T>> GetHealthData<T>(string elderEmail, Period period, DateTime date) where T : class;
    Task DeleteMax30102Data(DateTime currentDate, string arduino);
    Task DeleteGpsData(DateTime currentDate, string arduino);
    Task ComputeOutOfPerimeter(string arduino, Location location);
    Task<Location> GetLocation(DateTime currentTime, string arduino);
}