namespace HealthDevice.Services;

public interface ITimeZoneService
{
    DateTimeOffset UTCToLocalTime(TimeZoneInfo userTimeZone, DateTime utcNow);
    DateTime LocalTimeToUTC(TimeZoneInfo userTimeZone, DateTime utcNow);
}