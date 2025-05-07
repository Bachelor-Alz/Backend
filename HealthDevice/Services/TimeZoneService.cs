using System.Globalization;

namespace HealthDevice.Services;

public class TimeZoneService : ITimeZoneService
{
    private readonly ILogger<TimeZoneService> _logger;
    
    public TimeZoneService(ILogger<TimeZoneService> logger)
    {
        _logger = logger;
    }

    public DateTime GetCurrentTimeInUserTimeZone(TimeZoneInfo userTimeZone, DateTime utcNow)
    {
        DateTimeOffset userTime = TimeZoneInfo.ConvertTime(new DateTimeOffset(utcNow, TimeSpan.Zero), userTimeZone);
        return userTime.DateTime;
    }


    
    public DateTime GetCurrentTimeIntLocalTime(TimeZoneInfo userTimeZone, DateTime utcNow)
    {
        var utcTime = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utcTime, userTimeZone).ToUniversalTime();
    }

}