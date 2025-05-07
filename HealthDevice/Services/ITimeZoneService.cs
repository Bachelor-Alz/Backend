namespace HealthDevice.Services;

public interface ITimeZoneService
{
    DateTime GetCurrentTimeInUserTimeZone(TimeZoneInfo userTimeZone, DateTime utcNow);
    DateTime GetCurrentTimeIntLocalTime(TimeZoneInfo userTimeZone, DateTime utcNow);
}