using HealthDevice.DTO;
using HealthDevice.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class HealthServiceTests
{/*
    private readonly Mock<ILogger<HealthService>> _mockLogger;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IGetHealthData> _mockGetHealthDataService;
    private readonly HealthService _healthService;

    public HealthServiceTests()
    {
        _mockLogger = new Mock<ILogger<HealthService>>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockGetHealthDataService = new Mock<IGetHealthData>();
        _healthService = new HealthService(
            _mockLogger.Object,
            _mockRepositoryFactory.Object,
            Mock.Of<IEmailService>(),
            _mockGetHealthDataService.Object
        );
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
            new Heartrate { Timestamp = date.AddDays(-1), Avgrate = 70, Maxrate = 80, Minrate = 60 },
            new Heartrate { Timestamp = date.AddDays(-2), Avgrate = 75, Maxrate = 85, Minrate = 65 }
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
    */
}
