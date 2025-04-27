using HealthDevice.DTO;
using Microsoft.AspNetCore.Mvc;

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
    Task<ActionResult<List<FallDTO>>> GetFalls(string elderEmail, DateTime date, Period period);
    Task<ActionResult<List<ElderLocation>>> GetEldersLocation(string email);
    Task<ActionResult> SetPerimeter(int radius, string elderEmail);
    Task<ActionResult<List<Steps>>> GetSteps(string elderEmail, DateTime date, Period period);
    Task<ActionResult<List<Kilometer>>> GetDistance(string elderEmail, DateTime date, Period period);
    Task<ActionResult<List<PostHeartRate>>> GetHeartrate(string elderEmail, DateTime date, Period period);
    Task<ActionResult<List<PostSpo2>>> GetSpO2(string elderEmail, DateTime date, Period period);
}