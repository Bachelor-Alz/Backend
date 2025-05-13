using HealthDevice.DTO;
using HealthDevice.Models;
using Microsoft.AspNetCore.Mvc;
using StepsDTO = HealthDevice.DTO.StepsDTO;

namespace HealthDevice.Services;

public interface IHealthService
{
    Task<DistanceInfo> CalculateDistanceWalked(DateTime currentDate, string arduino);
    Task DeleteData<T>(DateTime currentDate, string arduino) where T : Sensor;
    Task DeleteGpsData(DateTime currentDate, string arduino);
    Task ComputeOutOfPerimeter(string arduino, Location location);
    Task<Location> GetLocation(DateTime currentTime, string arduino);
    Task<ActionResult<List<FallDTO>>> GetFalls(string elderId, DateTime date, Period period, TimeZoneInfo timezone);
    Task<ActionResult<List<ElderLocationDTO>>> GetEldersLocation(string caregiverId);
    Task<ActionResult<PerimeterDTO>> GetElderPerimeter(string elderId);
    Task<ActionResult> SetPerimeter(int radius, string elderId);
    Task<ActionResult<List<StepsDTO>>> GetSteps(string elderId, DateTime date, Period period, TimeZoneInfo timezone);
    Task<ActionResult<List<DistanceInfoDTO>>> GetDistance(string elderId, DateTime date, Period period, TimeZoneInfo timezone);
    Task<ActionResult<List<PostHeartRate>>> GetHeartrate(string elderId, DateTime date, Period period, TimeZoneInfo timezone);
    Task<ActionResult<List<PostSpO2>>> GetSpO2(string elderId, DateTime date, Period period, TimeZoneInfo timezone);
    Task<ActionResult<DashBoard>> GetDashboardData(string macAddress);
}