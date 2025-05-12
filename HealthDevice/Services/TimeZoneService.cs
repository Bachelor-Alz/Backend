// ReSharper disable SuggestVarOrType_SimpleTypes
namespace HealthDevice.Services;

public class TimeZoneService : ITimeZoneService
{
    public DateTimeOffset UTCToLocalTime(TimeZoneInfo userTimeZone, DateTime utcInput)
    {
        utcInput = DateTime.SpecifyKind(utcInput, DateTimeKind.Utc);

        TimeSpan utcOffset = userTimeZone.GetUtcOffset(utcInput);
        DateTimeOffset localDateTimeOffset = new DateTimeOffset(utcInput, TimeSpan.Zero).ToOffset(utcOffset);
        
        DateTime unspecifiedLocalTime = DateTime.SpecifyKind(utcInput, DateTimeKind.Unspecified);
        return new DateTimeOffset(unspecifiedLocalTime, localDateTimeOffset.Offset);
    }

    public DateTime LocalTimeToUTC(TimeZoneInfo userTimeZone, DateTime utcNow)
    {
        var localTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(utcNow, DateTimeKind.Unspecified), userTimeZone);
        return DateTime.SpecifyKind(localTime, DateTimeKind.Utc); 
    }
}