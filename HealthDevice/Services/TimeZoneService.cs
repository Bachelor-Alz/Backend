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
        DateTime userTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, userTimeZone).ToLocalTime();  // Convert to user timezone
        
        //Formated date
        string formattedDate = userTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
       // Use the formatted date to make a date with those attributes   
        DateTime formattedDateTime = DateTime.ParseExact(formattedDate, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        return formattedDateTime;
    }
    
    public DateTime GetCurrentTimeIntLocalTime(TimeZoneInfo userTimeZone, DateTime utcNow)
    {
        var utcTime = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utcTime, userTimeZone).ToUniversalTime();
    }

}