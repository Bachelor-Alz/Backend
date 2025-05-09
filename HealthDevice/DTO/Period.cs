namespace HealthDevice.DTO;

public enum Period
{
    Hour,
    Day,
    Week
}

public static class PeriodUtil
{
    public static DateTime GetEndDate(this Period period, DateTime date)
    {
        switch (period)
        {
            case Period.Hour:
                return new DateTime(date.Year, date.Month, date.Day, date.Hour, 59, 59).ToUniversalTime();
            case Period.Day:
                return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59).ToUniversalTime();
            case Period.Week:
                DateTime endOfWeek = date.AddDays(7 - (int)date.DayOfWeek).Date;
                return new DateTime(endOfWeek.Year, endOfWeek.Month, endOfWeek.Day, 23, 59, 59).ToUniversalTime();
            default:
                throw new ArgumentOutOfRangeException(nameof(period), "Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
        }
    }
}