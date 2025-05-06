using HealthDevice.DTO;
using HealthDevice.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class HealthServiceTests
{
    private readonly Mock<ILogger<HealthService>> _mockLogger;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IGetHealthDataService> _mockGetHealthDataService;
    private readonly HealthService _healthService;

    public HealthServiceTests()
    {
        _mockLogger = new Mock<ILogger<HealthService>>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockGetHealthDataService = new Mock<IGetHealthDataService>();
        _healthService = new HealthService(
            _mockLogger.Object,
            _mockRepositoryFactory.Object,
            Mock.Of<IEmailService>(),
            _mockGetHealthDataService.Object
        );

        _mockGetHealthDataService
            .Setup(s => s.GetHealthData<Heartrate>(It.IsAny<string>(), It.IsAny<Period>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Heartrate>
            {
                new Heartrate { Timestamp = DateTime.UtcNow, Avgrate = 70, Maxrate = 80, Minrate = 60 }
            });

        _mockGetHealthDataService
            .Setup(s => s.GetHealthData<Max30102>(It.IsAny<string>(), It.IsAny<Period>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Max30102>
            {
                new Max30102 { AvgHeartrate = 75, MaxHeartrate = 85, MinHeartrate = 65, Timestamp = DateTime.UtcNow }
            });
    }

    [Fact]
    public async Task GetHeartrate_ShouldReturnHourlyData_WhenPeriodIsHour()
    {
        // Arrange
        var elderEmail = "test@example.com";
        var date = new DateTime(2025, 5, 1, 10, 0, 0);
        var period = Period.Hour;

        var mockData = new List<Heartrate>
        {
            new Heartrate { Timestamp = date.AddMinutes(-30), Avgrate = 70, Maxrate = 80, Minrate = 60 },
            new Heartrate { Timestamp = date.AddMinutes(-15), Avgrate = 75, Maxrate = 85, Minrate = 65 }
        };

        _mockGetHealthDataService
            .Setup(s => s.GetHealthData<Heartrate>(elderEmail, period, It.IsAny<DateTime>()))
            .ReturnsAsync(mockData);

        // Act
        var result = await _healthService.GetHeartrate(elderEmail, date, period);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<Heartrate>>>(result);
        var heartrates = Assert.IsType<List<Heartrate>>(actionResult.Value);
        Assert.Equal(2, heartrates.Count);
        Assert.Equal(70, heartrates[0].Avgrate);
        Assert.Equal(75, heartrates[1].Avgrate);
    }

    [Fact]
    public async Task GetHeartrate_ShouldReturnDailyData_WhenPeriodIsDay()
    {
        // Arrange
        var elderEmail = "test@example.com";
        var date = new DateTime(2025, 5, 1);
        var period = Period.Day;

        var mockData = new List<Heartrate>
        {
            new Heartrate { Timestamp = date.AddHours(1), Avgrate = 70, Maxrate = 80, Minrate = 60 },
            new Heartrate { Timestamp = date.AddHours(2), Avgrate = 75, Maxrate = 85, Minrate = 65 }
        };

        _mockGetHealthDataService
            .Setup(s => s.GetHealthData<Heartrate>(elderEmail, period, It.IsAny<DateTime>()))
            .ReturnsAsync(mockData);

        // Act
        var result = await _healthService.GetHeartrate(elderEmail, date, period);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<Heartrate>>>(result);
        var heartrates = Assert.IsType<List<Heartrate>>(actionResult.Value);
        Assert.Equal(2, heartrates.Count);
        Assert.Equal(70, heartrates[0].Avgrate);
        Assert.Equal(75, heartrates[1].Avgrate);
    }

    [Fact]
    public async Task GetHeartrate_ShouldReturnWeeklyData_WhenPeriodIsWeek()
    {
        // Arrange
        var elderEmail = "test@example.com";
        var date = new DateTime(2025, 5, 1);
        var period = Period.Week;

        var mockData = new List<Heartrate>
        {
            new Heartrate { Timestamp = date.AddDays(-1), Avgrate = 70, Maxrate = 80, Minrate = 60 }, // 2025-04-30
            new Heartrate { Timestamp = date.AddDays(-2), Avgrate = 75, Maxrate = 85, Minrate = 65 },  // 2025-04-29
            new Heartrate { Timestamp = date.AddDays(-3), Avgrate = 80, Maxrate = 90, Minrate = 70 },  // 2025-04-28
            new Heartrate { Timestamp = date.AddDays(-4), Avgrate = 85, Maxrate = 95, Minrate = 75 },  // 2025-04-27
            new Heartrate { Timestamp = date.AddDays(-5), Avgrate = 90, Maxrate = 100, Minrate = 80 }, // 2025-04-26
            new Heartrate { Timestamp = date.AddDays(-6), Avgrate = 95, Maxrate = 105, Minrate = 85 }, // 2025-04-25
            new Heartrate { Timestamp = date.AddDays(-7), Avgrate = 100, Maxrate = 110, Minrate = 90 }, // 2025-04-24
            new Heartrate { Timestamp = date.AddDays(-8), Avgrate = 105, Maxrate = 115, Minrate = 95 }, // 2025-04-23
            new Heartrate { Timestamp = date.AddDays(-9), Avgrate = 110, Maxrate = 120, Minrate = 100 } // 2025-04-22
        };

        _mockGetHealthDataService
            .Setup(s => s.GetHealthData<Heartrate>(elderEmail, period, It.IsAny<DateTime>()))
            .ReturnsAsync(mockData);

        // Act
        var result = await _healthService.GetHeartrate(elderEmail, date, period);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<Heartrate>>>(result);
        var heartrates = Assert.IsType<List<Heartrate>>(actionResult.Value);
        Assert.Equal(7, heartrates.Count);
        Assert.Equal(100, heartrates[0].Avgrate);
        Assert.Equal(70, heartrates[6].Avgrate);  
    }
}
