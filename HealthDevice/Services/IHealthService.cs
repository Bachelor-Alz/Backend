using HealthDevice.DTO;
using HealthDevice.Models;
using Microsoft.AspNetCore.Mvc;

namespace HealthDevice.Services;

public interface IHealthService
{
    Task<List<Heartrate>> CalculateHeartRate(DateTime currentDate, string address);
    Task<List<Spo2>> CalculateSpo2(DateTime currentDate, string address);
    Task<DistanceInfo> CalculateDistanceWalked(DateTime currentDate, string arduino);
    Task DeleteMax30102Data(DateTime currentDate, string arduino);
    Task DeleteGpsData(DateTime currentDate, string arduino);
    Task ComputeOutOfPerimeter(string arduino, Location location);
    Task<Location> GetLocation(DateTime currentTime, string arduino);
    Task<ActionResult<List<FallDTO>>> GetFalls(string elderEmail, DateTime date, Period period, TimeZoneInfo timezone);
    Task<ActionResult<List<ElderLocationDTO>>> GetEldersLocation(string email);
    Task<ActionResult> SetPerimeter(int radius, string elderEmail);
    Task<ActionResult<List<Steps>>> GetSteps(string elderEmail, DateTime date, Period period, TimeZoneInfo timezone);
    Task<ActionResult<List<DistanceInfo>>> GetDistance(string elderEmail, DateTime date, Period period, TimeZoneInfo timezone);
    Task<ActionResult<List<PostHeartRate>>> GetHeartrate(string elderEmail, DateTime date, Period period, TimeZoneInfo timezone);
    Task<ActionResult<List<PostSpO2>>> GetSpO2(string elderEmail, DateTime date, Period period, TimeZoneInfo timezone);
}