using System.Globalization;

namespace HealthDevice.Services;

public class TimeZoneService : ITimeZoneService
{
    private readonly ILogger<TimeZoneService> _logger;
    
    public TimeZoneService(ILogger<TimeZoneService> logger)
    {
        _logger = logger;
    }

    public DateTimeOffset GetCurrentTimeInUserTimeZone(TimeZoneInfo userTimeZone, DateTime utcInput)
    {
        // Ensure the input is in UTC
        if (utcInput.Kind != DateTimeKind.Utc)
            utcInput = DateTime.SpecifyKind(utcInput, DateTimeKind.Utc);

        // Convert to DateTimeOffset with UTC and apply the user time zone offset
        DateTimeOffset utcDateTimeOffset = new DateTimeOffset(utcInput, TimeSpan.Zero);
        return TimeZoneInfo.ConvertTime(utcDateTimeOffset, userTimeZone);
    }
    
    public DateTime GetCurrentTimeIntLocalTime(TimeZoneInfo userTimeZone, DateTime utcNow)
    {
        var utcTime = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
    
        // Convert from UTC to local time
        DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, userTimeZone);
    
        // Return local time (no need to convert back to UTC)
        return localTime;
    }
}