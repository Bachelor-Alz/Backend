namespace HealthDevice.Services;

public class TimeZoneService : ITimeZoneService
{
    public DateTime GetCurrentTimeInUserTimeZone(TimeZoneInfo userTimeZone, DateTime utcNow)
    {
        DateTime userTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, userTimeZone);  // Convert to user timezone
        
        DateTime startOfDay = userTime.Date;  // 00:00:00 of the local day
        DateTime endOfDay = startOfDay.AddDays(1).AddSeconds(-1);  // 23:59:59 of the local day
        
        return userTime < startOfDay ? startOfDay : userTime > endOfDay ? endOfDay : userTime;
    }
}