namespace HealthDevice.Services;

public interface ITimeZoneService
{
    DateTime GetCurrentTimeInUserTimeZone(TimeZoneInfo userTimeZone, DateTime utcNow);
}