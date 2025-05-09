namespace HealthDevice.Services;

public class TimeZoneService : ITimeZoneService
{
    public DateTimeOffset UTCToLocalTime(TimeZoneInfo userTimeZone, DateTime utcInput)
    {
        utcInput = DateTime.SpecifyKind(utcInput, DateTimeKind.Utc);

        TimeSpan utcOffset = userTimeZone.GetUtcOffset(utcInput);
        DateTimeOffset localDateTimeOffset = new DateTimeOffset(utcInput, TimeSpan.Zero).ToOffset(utcOffset);

        DateTime LocalTime = utcInput - localDateTimeOffset.Offset;
        DateTime unspecifiedLocalTime = DateTime.SpecifyKind(LocalTime, DateTimeKind.Unspecified);
        return new DateTimeOffset(unspecifiedLocalTime, localDateTimeOffset.Offset);
    }

    public DateTime LocalTimeToUTC(TimeZoneInfo userTimeZone, DateTime utcNow)
    {
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc), userTimeZone);
        return DateTime.SpecifyKind(localTime, DateTimeKind.Utc); // Ensure the returned DateTime is UTC
    }
}