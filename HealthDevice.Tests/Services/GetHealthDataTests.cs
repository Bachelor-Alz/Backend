using HealthDevice.DTO;
using HealthDevice.Models;
using HealthDevice.Services;
using Microsoft.Extensions.Logging;
using Moq;
using HealthDevice.Tests.Helpers;

public class GetHealthDataTests
{
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<ILogger<GetHealthDataService>> _mockLogger;
    private readonly Mock<ITimeZoneService> _mockTimeZoneService;
    private readonly Mock<IRepository<Elder>> _mockElderRepository;
    private readonly GetHealthDataService _getHealthDataService;

    public GetHealthDataTests()
    {
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockLogger = new Mock<ILogger<GetHealthDataService>>();
        _mockTimeZoneService = new Mock<ITimeZoneService>();
        _mockElderRepository = new Mock<IRepository<Elder>>();

        _getHealthDataService = new GetHealthDataService(
            _mockRepositoryFactory.Object,
            _mockLogger.Object,
            _mockTimeZoneService.Object,
            _mockElderRepository.Object
        );
    }

    // Helper method to create an IQueryable mock
    private static IQueryable<T> CreateMockQueryable<T>(IEnumerable<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var testDbAsyncEnumerable = new TestDbAsyncEnumerable<T>(queryable.Expression);
        var testDbAsyncQueryProvider = new TestDbAsyncQueryProvider<T>(queryable.Provider);

        var mock = new Mock<IQueryable<T>>();
        mock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(testDbAsyncQueryProvider);
        mock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
        mock.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
           .Returns(new TestDbAsyncEnumerable<T>(data).GetAsyncEnumerator());

        return mock.Object;
    }


    [Fact]
    public async Task GetHealthData_ElderNotFound_ReturnsEmptyList()
    {
        // Arrange
        _mockElderRepository.Setup(r => r.Query()).Returns(CreateMockQueryable(new List<Elder>()));

        // Act
        var result = await _getHealthDataService.GetHealthData<Spo2>("test@elder.com", Period.Day, DateTime.UtcNow, TimeZoneInfo.Utc);

        // Assert
        Assert.Empty(result);
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No elder found or Arduino is not set")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GetHealthData_ElderWithoutArduino_ReturnsEmptyList()
    {
        // Arrange
        var elder = new Elder { Name = "Test Elder", Email = "elder@test.com", MacAddress = null, Id = "elderId" };
        _mockElderRepository.Setup(r => r.Query()).Returns(CreateMockQueryable(new List<Elder> { elder }));

        // Act
        var result = await _getHealthDataService.GetHealthData<Spo2>("elderId", Period.Day, DateTime.UtcNow, TimeZoneInfo.Utc);

        // Assert
        Assert.Empty(result);
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No elder found or Arduino is not set")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GetHealthData_ValidData_ReturnsData()
    {
        // Arrange
        var macAddress = "test-mac-address";
        var elder = new Elder { Name = "Test Elder", Email = "elder@test.com", MacAddress = macAddress, OutOfPerimeter = false, Id = "elderId" };
        var testTime = new DateTime(2025, 5, 14, 12, 0, 0, DateTimeKind.Utc);

        var sensorData = new List<Heartrate>
        {
            new() { Maxrate = 80, Minrate = 60, Avgrate = 70, Timestamp = testTime.AddHours(-1).AddSeconds(1), MacAddress = macAddress },
            new() { Maxrate = 82, Minrate = 62, Avgrate = 72, Timestamp = testTime.AddMinutes(-1).AddSeconds(-1), MacAddress = macAddress }
        };

        _mockElderRepository.Setup(r => r.Query()).Returns(CreateMockQueryable(new List<Elder> { elder }));

        var mockSensorRepository = new Mock<IRepository<Heartrate>>();
        mockSensorRepository.Setup(r => r.Query()).Returns(CreateMockQueryable(sensorData));
        _mockRepositoryFactory.Setup(f => f.GetRepository<Heartrate>()).Returns(mockSensorRepository.Object);

        _mockTimeZoneService.Setup(t => t.LocalTimeToUTC(It.IsAny<TimeZoneInfo>(), It.IsAny<DateTime>()))
                            .Returns((TimeZoneInfo tz, DateTime dt) => dt);

        // Act
        var result = await _getHealthDataService.GetHealthData<Heartrate>("elderId", Period.Hour, testTime, TimeZoneInfo.Utc);

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(sensorData.Count, result.Count);
        Assert.Equal(sensorData[0].MacAddress, result[0].MacAddress);
        Assert.Equal(sensorData[1].MacAddress, result[1].MacAddress);


        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Retrieved {sensorData.Count} records for type {typeof(Heartrate).Name}")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GetHealthData_InvalidPeriod_ThrowsException()
    {
        // Arrange
        var macAddress = "test-mac-address";
        var elder = new Elder { Name = "Test Elder", Email = "elder@test.com", MacAddress = macAddress, OutOfPerimeter = false };
        _mockElderRepository.Setup(r => r.Query()).Returns(CreateMockQueryable(new List<Elder> { elder }));

        _mockTimeZoneService.Setup(t => t.LocalTimeToUTC(It.IsAny<TimeZoneInfo>(), It.IsAny<DateTime>()))
                             .Returns((TimeZoneInfo tz, DateTime dt) => dt);


        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _getHealthDataService.GetHealthData<Heartrate>("elder@test.com", (Period)42, DateTime.UtcNow, TimeZoneInfo.Utc)
        );
    }
    [Fact]
    public async Task GetHealthData_PeriodDay_FiltersCorrectly()
    {
        // Arrange
        var macAddress = "test-mac-address";
        var elder = new Elder { Name = "Test Elder", Email = "elder@test.com", MacAddress = macAddress, OutOfPerimeter = false, Id = "elderId" };
        var testTime = new DateTime(2025, 5, 14, 12, 0, 0, DateTimeKind.Utc);

        var sensorData = new List<Heartrate>
        {
            new() { Avgrate = 70, Maxrate = 80, Minrate = 60, Timestamp = testTime.AddHours(-1), MacAddress = macAddress },
            new() { Avgrate = 72, Maxrate = 82, Minrate = 62, Timestamp = testTime.AddHours(-2), MacAddress = macAddress },
            new() { Avgrate = 72, Maxrate = 82, Minrate = 62, Timestamp = testTime.AddDays(2), MacAddress = macAddress }

        };

        _mockElderRepository.Setup(r => r.Query()).Returns(CreateMockQueryable(new List<Elder> { elder }));

        var mockSensorRepository = new Mock<IRepository<Heartrate>>();
        mockSensorRepository.Setup(r => r.Query()).Returns(CreateMockQueryable(sensorData));
        _mockRepositoryFactory.Setup(f => f.GetRepository<Heartrate>()).Returns(mockSensorRepository.Object);

        _mockTimeZoneService.Setup(t => t.LocalTimeToUTC(It.IsAny<TimeZoneInfo>(), It.IsAny<DateTime>()))
                            .Returns((TimeZoneInfo tz, DateTime dt) => dt);

        // Act
        var result = await _getHealthDataService.GetHealthData<Heartrate>("elderId", Period.Day, testTime, TimeZoneInfo.Utc);

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetHealthData_PeriodDay_FiltersCorrectlyTwo()
    {
        // Arrange
        var macAddress = "test-mac-address";
        var elder = new Elder { Name = "Test Elder", Email = "elder@test.com", MacAddress = macAddress, OutOfPerimeter = false, Id = "elderId" };
        var testTime = new DateTime(2025, 5, 14, 12, 0, 0, DateTimeKind.Utc);

        var sensorData = new List<Heartrate>
        {
            new() { Maxrate = 80, Minrate = 60, Avgrate = 70, Timestamp = testTime.AddHours(-1), MacAddress = macAddress },
            new() { Maxrate = 82, Minrate = 62, Avgrate = 72, Timestamp = testTime.AddHours(-2), MacAddress = macAddress },
            new() { Maxrate = 82, Minrate = 62, Avgrate = 72, Timestamp = testTime.AddDays(2), MacAddress = macAddress }
        };

        _mockElderRepository.Setup(r => r.Query()).Returns(CreateMockQueryable(new List<Elder> { elder }));

        var mockSensorRepository = new Mock<IRepository<Heartrate>>();
        mockSensorRepository.Setup(r => r.Query()).Returns(CreateMockQueryable(sensorData));
        _mockRepositoryFactory.Setup(f => f.GetRepository<Heartrate>()).Returns(mockSensorRepository.Object);

        _mockTimeZoneService.Setup(t => t.LocalTimeToUTC(It.IsAny<TimeZoneInfo>(), It.IsAny<DateTime>()))
                            .Returns((TimeZoneInfo tz, DateTime dt) => dt);

        // Act
        var result = await _getHealthDataService.GetHealthData<Heartrate>("elderId", Period.Day, testTime, TimeZoneInfo.Utc);

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count);
    }
}
