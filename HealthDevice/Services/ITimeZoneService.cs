namespace HealthDevice.Services;

public interface ITimeZoneService
{
    DateTimeOffset GetCurrentTimeInUserTimeZone(TimeZoneInfo userTimeZone, DateTime utcNow);
    DateTime GetCurrentTimeIntLocalTime(TimeZoneInfo userTimeZone, DateTime utcNow);
}