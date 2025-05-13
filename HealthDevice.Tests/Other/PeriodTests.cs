using HealthDevice.DTO;

public class PeriodTests
{
    [Fact]
    public void GetEndDate_PeriodHour_ReturnsCorrectEndDate()
    {
        // Arrange
        var input = new DateTime(2025, 5, 12, 10, 30, 0);
        var expected = new DateTime(2025, 5, 12, 10, 59, 59).ToUniversalTime();

        // Act
        var result = PeriodUtil.GetEndDate(Period.Hour, input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetEndDate_PeriodDay_ReturnsCorrectEndDate()
    {
        // Arrange
        var input = new DateTime(2025, 5, 12, 10, 30, 0);
        var expected = new DateTime(2025, 5, 12, 23, 59, 59).ToUniversalTime();

        // Act
        var result = PeriodUtil.GetEndDate(Period.Day, input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetEndDate_PeriodWeek_ReturnsCorrectEndDate()
    {
        // Arrange
        var input = new DateTime(2025, 5, 12, 10, 30, 0); // Monday
        var expected = new DateTime(2025, 5, 18, 23, 59, 59).ToUniversalTime(); // Sunday

        // Act
        var result = PeriodUtil.GetEndDate(Period.Week, input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AggregateByPeriod_BasicAggregation_ReturnsCorrectResults()
    {
        // Arrange
        var data = new List<DateTime>
        {
            new(2025, 5, 12, 10, 0, 0, DateTimeKind.Utc),
            new(2025, 5, 12, 10, 5, 0, DateTimeKind.Utc),
            new(2025, 5, 12, 10, 10, 0, DateTimeKind.Utc)
        };

        Period period = Period.Hour;
        DateTime referenceDate = new(2025, 5, 12, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var result = PeriodUtil.AggregateByPeriod(
            data,
            period,
            referenceDate,
            timestamp => timestamp,
            (group, slot) => new { Slot = slot, Count = group.Count() },
            slot => new { Slot = slot, Count = 0 }
        );

        // Assert
        Assert.Equal(12, result.Count); // 12 slots in an hour
        var slotsWithData = result.Where(r => r.Count > 0).Select(r => r.Slot).ToList();

        // Assert that slots with data match the input data
        Assert.Contains(new(2025, 5, 12, 10, 0, 0, DateTimeKind.Utc), slotsWithData);
        Assert.Contains(new(2025, 5, 12, 10, 5, 0, DateTimeKind.Utc), slotsWithData);
        Assert.Contains(new(2025, 5, 12, 10, 10, 0, DateTimeKind.Utc), slotsWithData);

        // Assert that slots without data have a count of 0
        Assert.All(result.Where(r => !slotsWithData.Contains(r.Slot)), r => Assert.Equal(0, r.Count));
    }

    [Fact]
    public void AggregateByPeriod_EmptyData_ReturnsDefaultResults()
    {
        // Arrange
        var data = new List<DateTime>();
        Period period = Period.Week;
        DateTime referenceDate = new(2025, 5, 12, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var result = PeriodUtil.AggregateByPeriod(
            data,
            period,
            referenceDate,
            timestamp => timestamp,
            (group, slot) => new { Slot = slot, Count = group.Count() },
            slot => new { Slot = slot, Count = 0 }
        );

        // Assert
        Assert.Equal(7, result.Count);
        Assert.All(result, r => Assert.Equal(0, r.Count));
    }

    [Fact]
    public void AggregateByPeriod_PartialSlotCoverage_ReturnsMixedResults()
    {
        // Arrange
        var data = new List<DateTime>
        {
            new(2025, 5, 12, 10, 0, 0, DateTimeKind.Utc),
            new(2025, 5, 12, 11, 0, 0, DateTimeKind.Utc)
        };

        Period period = Period.Day;
        DateTime referenceDate = new(2025, 5, 12, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = PeriodUtil.AggregateByPeriod(
            data,
            period,
            referenceDate,
            timestamp => timestamp,
            (group, slot) => new { Slot = slot, Count = group.Count() },
            slot => new { Slot = slot, Count = 0 }
        );

        // Assert
        Assert.Equal(24, result.Count);
        var slotsWithData = result.Where(r => r.Count > 0).Select(r => r.Slot).ToList();
        Assert.Contains(new(2025, 5, 12, 10, 0, 0, DateTimeKind.Utc), slotsWithData);
        Assert.Contains(new(2025, 5, 12, 11, 0, 0, DateTimeKind.Utc), slotsWithData);
        Assert.All(result.Where(r => !slotsWithData.Contains(r.Slot)), r => Assert.Equal(0, r.Count));
    }

    [Fact]
    public void AggregateByPeriod_EdgeCaseDataAtSlotBoundary_ReturnsCorrectResults()
    {
        // Arrange
        var data = new List<DateTime>
        {
            new(2025, 5, 12, 10, 59, 59, DateTimeKind.Utc), // Last second of the hour
            new(2025, 5, 12, 11, 0, 0, DateTimeKind.Utc) // First second of the next hour
        };

        Period period = Period.Hour;
        DateTime referenceDate = new DateTime(2025, 5, 12, 10, 0, 0, DateTimeKind.Utc);

        // Act
        var result = PeriodUtil.AggregateByPeriod(
            data,
            period,
            referenceDate,
            timestamp => timestamp,
            (group, slot) => new { Slot = slot, Count = group.Count() },
            slot => new { Slot = slot, Count = 0 }
        );

        // Assert
        Assert.Equal(12, result.Count);
        var slotsWithData = result.Where(r => r.Count > 0).Select(r => r.Slot).ToList();
        Assert.Contains(referenceDate.AddMinutes(55), slotsWithData); // Last slot of the hour
        Assert.DoesNotContain(referenceDate.AddHours(1), slotsWithData); // First slot of the next hour
        Assert.All(result.Where(r => !slotsWithData.Contains(r.Slot)), r => Assert.Equal(0, r.Count));
    }
}