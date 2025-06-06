﻿using HealthDevice.Services;

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
                 DateTime sunday = date.Date.AddDays((7 - (int)date.DayOfWeek) % 7);
                 return new DateTime(sunday.Year, sunday.Month, sunday.Day, 23, 59, 59).ToUniversalTime();
            default:
                throw new ArgumentOutOfRangeException(nameof(period), "Invalid period specified. Valid values are 'Hour', 'Day', or 'Week'.");
        }
    }
    private static DateTime GetSlotStart(this Period period, DateTime date, TimeZoneInfo timezone, ITimeZoneService timezoneService)
    {
        switch (period)
        {
            case Period.Hour:
                int minuteSlot = date.Minute / 5 * 5;
                return new DateTime(date.Year, date.Month, date.Day, date.Hour, minuteSlot, 0, DateTimeKind.Utc);
            case Period.Day:
                return  new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0, DateTimeKind.Unspecified);
            case Period.Week:
                return timezoneService.LocalTimeToUTC(timezone, date.Date);
            default:
                throw new ArgumentOutOfRangeException(nameof(period), period, null);
        }
    }
    private static int GetMaxDataSlots(this Period period)
    {
        return period switch
        {
            Period.Hour => 60 / 5, // 5-minute intervals in an hour
            Period.Day => 24,      // 24 slots, one for each hour of the day
            Period.Week => 7,      // 7 slots, one for each day of the week
            _ => throw new ArgumentOutOfRangeException(nameof(period), period, null)
        };
    }
    private static IEnumerable<DateTime> GetExpectedSlots(this Period period, DateTime referenceDate, TimeZoneInfo timezone, ITimeZoneService timezoneService)
    {
        int max = period.GetMaxDataSlots();
        switch (period)
        {
            case Period.Hour:
                var hourStart = new DateTime(referenceDate.Year, referenceDate.Month, referenceDate.Day, referenceDate.Hour, 0, 0, DateTimeKind.Unspecified);
                hourStart = timezoneService.LocalTimeToUTC(timezone, hourStart); 
                for (int i = 0; i < max; i++)
                    yield return hourStart.AddMinutes(i * 5);
                break;
            case Period.Day:
                var dayStart = new DateTime(referenceDate.Year, referenceDate.Month, referenceDate.Day, 0, 0, 0, DateTimeKind.Unspecified);
                 dayStart = timezoneService.LocalTimeToUTC(timezone, dayStart);
                for (int i = 0; i < max; i++)
                    yield return dayStart.AddHours(i);
                break;
            case Period.Week:
                var weekStart = referenceDate.Date; 
                weekStart = weekStart.AddDays(-((int)weekStart.DayOfWeek + 6) % 7);
                weekStart = timezoneService.LocalTimeToUTC(timezone, weekStart); 
                for (int i = 0; i < max; i++) 
                    yield return weekStart.AddDays(i);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(period), period, null);
        }
    }

    /// <summary>
    /// Aggregates data into periods based on a specified time slot and performs custom aggregation according to the "aggregateFunc" function.
    /// The aggregation is done by grouping the data by the time slot (which is determined based on the Period and TimeZone)
    /// and applying the provided aggregation function to each group.
    /// If a time slot has no data, the "defaultFactory" function is used to generate a default result for that slot.
    /// The amount of time slots is determined by the "period" parameter. According to GetMaxDataSlots, the period can be Hour, Day or Week.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the input data.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the aggregation.</typeparam>
    /// <param name="data">The collection of data to be aggregated (timestamps should likely be in UTC).</param>
    /// <param name="period">The period definition used to determine time slots for grouping and output.</param>
    /// <param name="referenceDate">The reference date used to calculate expected slots (should likely be in Local time).</param>
    /// <param name="timezone">The user's timezone.</param>
    /// <param name="timezoneService">The service for handling timezone conversions.</param>
    /// <param name="timestampSelector">A function to extract the timestamp from each data element.</param>
    /// <param name="aggregateFunc">
    /// A function to aggregate the data in each time slot. Takes the grouped data and the slot start time (UTC) as parameters.
    /// </param>
    /// <param name="defaultFactory">
    /// A function to generate a default result for time slots with no data. Takes the slot start time (UTC) as a parameter.
    /// </param>
    /// <returns>
    /// A list of aggregated results, one for each expected time slot.
    /// </returns>
    public static List<TResult> AggregateByPeriod<TSource, TResult>(
        IEnumerable<TSource> data,
        Period period,
        DateTime referenceDate,
        TimeZoneInfo timezone,
        ITimeZoneService timezoneService,
        Func<TSource, DateTime> timestampSelector, 
        Func<IEnumerable<TSource>, DateTime, TResult> aggregateFunc,
        Func<DateTime, TResult> defaultFactory)
    {
        Dictionary<DateTime, IGrouping<DateTime, TSource>> grouped = data
            .GroupBy(x => period.GetSlotStart(timestampSelector(x), timezone, timezoneService))
            .ToDictionary(g => g.Key, g => g);
        
        IEnumerable<DateTime> slots = period.GetExpectedSlots(referenceDate, timezone, timezoneService);
        
        return slots.Select(slot => grouped.TryGetValue(slot, out IGrouping<DateTime, TSource>? group)
                ? aggregateFunc(group, slot)
                : defaultFactory(slot)) 
            .ToList();
    }

    public static DateTime GetEarlierDate(DateTime date, Period period) => period switch
    {
        Period.Hour => date - TimeSpan.FromHours(1),
        Period.Day => date.Date, // Gets the start of the current day (local)
        // Gets the start of the current week (Monday local)
        Period.Week => date.Date.AddDays(- (((int)date.Date.DayOfWeek + 6) % 7) ),
        _ => throw new ArgumentException("Invalid period specified")
    };
}
