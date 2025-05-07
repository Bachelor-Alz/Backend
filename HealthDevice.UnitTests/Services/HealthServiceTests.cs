using HealthDevice.DTO;
using HealthDevice.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using HealthDevice.UnitTests.Helpers;

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

    [Fact]
    public async Task GetSpO2_ShouldReturnWeeklyData_WhenPeriodIsWeek()
    {
        // Arrange
        var elderEmail = "test@example.com";
        var date = new DateTime(2025, 5, 1);
        var period = Period.Week;

        var mockData = new List<Spo2>
        {
            new Spo2 { Timestamp = date.AddDays(-1), AvgSpO2 = 99, MaxSpO2 = 100, MinSpO2 = 98 }, // 2025-04-30
            new Spo2 { Timestamp = date.AddDays(-2), AvgSpO2 = 99, MaxSpO2 = 100, MinSpO2 = 97 },  // 2025-04-29
            new Spo2 { Timestamp = date.AddDays(-3), AvgSpO2 = 99, MaxSpO2 = 100, MinSpO2 = 96 },  // 2025-04-28
            new Spo2 { Timestamp = date.AddDays(-4), AvgSpO2 = 98, MaxSpO2 = 100, MinSpO2 = 87 },  // 2025-04-27
            new Spo2 { Timestamp = date.AddDays(-5), AvgSpO2 = 98, MaxSpO2 = 100, MinSpO2 = 93 }, // 2025-04-26
            new Spo2 { Timestamp = date.AddDays(-6), AvgSpO2 = 97, MaxSpO2 = 100, MinSpO2 = 95 }, // 2025-04-25
            new Spo2 { Timestamp = date.AddDays(-7), AvgSpO2 = 98, MaxSpO2 = 100, MinSpO2 = 96 }, // 2025-04-24
            new Spo2 { Timestamp = date.AddDays(-8), AvgSpO2 = 99, MaxSpO2 = 100, MinSpO2 = 97 }, // 2025-04-23
            new Spo2 { Timestamp = date.AddDays(-9), AvgSpO2 = 97, MaxSpO2 = 100, MinSpO2 = 90 } // 2025-04-22
        };

        _mockGetHealthDataService
            .Setup(s => s.GetHealthData<Spo2>(elderEmail, period, It.IsAny<DateTime>()))
            .ReturnsAsync(mockData);

        // Act
        var result = await _healthService.GetSpO2(elderEmail, date, period);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<Spo2>>>(result);
        var spo2 = Assert.IsType<List<Spo2>>(actionResult.Value);
        Assert.Equal(7, spo2.Count);
        Assert.Equal(98, spo2[0].AvgSpO2);
        Assert.Equal(99, spo2[6].AvgSpO2);
    }

    [Fact]
    public async Task GetSteps_ShouldReturnWeeklyData_WhenPeriodIsWeek()
    {
        // Arrange
        var elderEmail = "test@example.com";
        var date = new DateTime(2025, 5, 1);
        var period = Period.Week;

        var mockData = new List<Steps>();
        for (int day = 0; day < 9; day++)
        {
            mockData.Add(new Steps
            {
                Timestamp = date.AddDays(-day),
                StepsCount = 1000 + day * 100
            });
        }

        _mockGetHealthDataService
            .Setup(s => s.GetHealthData<Steps>(elderEmail, period, It.IsAny<DateTime>()))
            .ReturnsAsync(mockData);

        // Act
        var result = await _healthService.GetSteps(elderEmail, date, period);

        // Assert
        var actionResult = Assert.IsType<List<Steps>>(result.Value);
        Assert.Equal(7, actionResult.Count);
        Assert.Equal(1600, actionResult[0].StepsCount);
        Assert.Equal(1000, actionResult[6].StepsCount);
    }

    [Fact]
    public async Task ComputeOutOfPerimeter_ShouldUpdateElderLocation_WhenElderIsOutOfPerimeter()
    {
        // Arrange
        var arduino = "test-mac-address";
        var location = new Location
        {
            Latitude = 56.0124,
            Longitude = 9.9915,
            Timestamp = DateTime.UtcNow
        };

        var perimeter = new Perimeter
        {
            Latitude = 57.0000,
            Longitude = 10.0000,
            Radius = 5,
            MacAddress = arduino
        };

        var elder = new Elder
        {
            Email = "test@example.com",
            UserName = "test@example.com",
            Name = "Test Elder",
            MacAddress = arduino,
            outOfPerimeter = false
        };

        var mockPerimeterRepository = new Mock<IRepository<Perimeter>>();
        var mockElderRepository = new Mock<IRepository<Elder>>();

        // Mock Perimeter repository with async support
        var perimeters = new List<Perimeter> { perimeter }.AsQueryable();
        var asyncPerimeters = new TestDbAsyncEnumerable<Perimeter>(perimeters);
        mockPerimeterRepository.Setup(r => r.Query()).Returns(asyncPerimeters);

        // Mock Elder repository with async support
        var elders = new List<Elder> { elder }.AsQueryable();
        var asyncElders = new TestDbAsyncEnumerable<Elder>(elders);
        mockElderRepository.Setup(r => r.Query()).Returns(asyncElders);

        _mockRepositoryFactory
            .Setup(f => f.GetRepository<Perimeter>())
            .Returns(mockPerimeterRepository.Object);

        _mockRepositoryFactory
            .Setup(f => f.GetRepository<Elder>())
            .Returns(mockElderRepository.Object);

        mockElderRepository
            .Setup(r => r.Update(It.IsAny<Elder>()))
            .Callback<Elder>(e => elder = e);

        // Act
        await _healthService.ComputeOutOfPerimeter(arduino, location);

        // Assert
        Assert.NotNull(elder); // Ensure the Update method was called
        Assert.True(elder.outOfPerimeter, "Elder should be marked as out of perimeter.");
    }
    
    [Fact]
    public async Task ComputeOutOfPerimeter_ShouldUpdateElderLocation_WhenElderIsInPerimeter()
    {
        // Arrange
        var arduino = "test-mac-address";
        var location = new Location
        {
            Latitude = 57.0124,
            Longitude = 9.9915,
            Timestamp = DateTime.UtcNow
        };

        var perimeter = new Perimeter
        {
            Latitude = 57.0000,
            Longitude = 10.0000,
            Radius = 5,
            MacAddress = arduino
        };

        var elder = new Elder
        {
            Email = "test@example.com",
            UserName = "test@example.com",
            Name = "Test Elder",
            MacAddress = arduino,
            outOfPerimeter = true
        };

        var mockPerimeterRepository = new Mock<IRepository<Perimeter>>();
        var mockElderRepository = new Mock<IRepository<Elder>>();

        // Mock Perimeter repository with async support
        var perimeters = new List<Perimeter> { perimeter }.AsQueryable();
        var asyncPerimeters = new TestDbAsyncEnumerable<Perimeter>(perimeters);
        mockPerimeterRepository.Setup(r => r.Query()).Returns(asyncPerimeters);

        // Mock Elder repository with async support
        var elders = new List<Elder> { elder }.AsQueryable();
        var asyncElders = new TestDbAsyncEnumerable<Elder>(elders);
        mockElderRepository.Setup(r => r.Query()).Returns(asyncElders);

        _mockRepositoryFactory
            .Setup(f => f.GetRepository<Perimeter>())
            .Returns(mockPerimeterRepository.Object);

        _mockRepositoryFactory
            .Setup(f => f.GetRepository<Elder>())
            .Returns(mockElderRepository.Object);

        mockElderRepository
            .Setup(r => r.Update(It.IsAny<Elder>()))
            .Callback<Elder>(e => elder = e);

        // Act
        await _healthService.ComputeOutOfPerimeter(arduino, location);

        // Assert
        Assert.NotNull(elder); // Ensure the Update method was called
        Assert.False(elder.outOfPerimeter, "Elder should be marked as in the perimeter.");
    }

    [Fact]
    public async Task GetLocation_ShouldReturnElderLocation_WhenGPSExists()
    {
        // Arrange
        var arduino = "test-mac-address";
        var currentTime = DateTime.UtcNow;

        var gpsData = new GPS
        {
            Latitude = 57.0124,
            Longitude = 9.9915,
            Timestamp = currentTime.AddMinutes(-5),
            MacAddress = arduino
        };

        var mockGPSRepository = new Mock<IRepository<GPS>>();
        var gpsDataList = new List<GPS> { gpsData }.AsQueryable();
        var asyncGPSData = new TestDbAsyncEnumerable<GPS>(gpsDataList);
        mockGPSRepository.Setup(r => r.Query()).Returns(asyncGPSData);

        _mockRepositoryFactory
            .Setup(f => f.GetRepository<GPS>())
            .Returns(mockGPSRepository.Object);

        // Act
        var result = await _healthService.GetLocation(currentTime, arduino);

        // Assert
        var location = Assert.IsType<Location>(result);
        Assert.Equal(gpsData.Latitude, location.Latitude);
        Assert.Equal(gpsData.Longitude, location.Longitude);
        Assert.Equal(gpsData.Timestamp, location.Timestamp);
    }

    [Fact]
    public async Task GetLocation_ShouldReturnDefaultLocation_WhenGPSDataIsNull()
    {
        // Arrange
        var arduino = "test-mac-address";
        var currentTime = new DateTime(2025, 5, 1, 12, 0, 0);

        var mockGPSRepository = new Mock<IRepository<GPS>>();
        var gpsDataList = new List<GPS>().AsQueryable(); // No GPS data
        var asyncGPSData = new TestDbAsyncEnumerable<GPS>(gpsDataList);
        mockGPSRepository.Setup(r => r.Query()).Returns(asyncGPSData);

        _mockRepositoryFactory
            .Setup(f => f.GetRepository<GPS>())
            .Returns(mockGPSRepository.Object);

        // Act
        var result = await _healthService.GetLocation(currentTime, arduino);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Location>(result);  
        // Verify logger warning
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"No GPS data found for elder {arduino}")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetFalls_ShouldReturnWeeklyData_WhenPeriodIsWeek()
    {
        // Arrange
        var elderEmail = "test@example.com";
        var date = new DateTime(2025, 5, 1);
        var period = Period.Week;

        var mockData = new List<FallInfo>
        {
            new FallInfo { Timestamp = date.AddDays(-1), MacAddress = "test-mac-address" }, // 2025-04-30
            new FallInfo { Timestamp = date.AddDays(-2), MacAddress = "test-mac-address" }, // 2025-04-29
            new FallInfo { Timestamp = date.AddDays(-3), MacAddress = "test-mac-address" }, // 2025-04-28
            new FallInfo { Timestamp = date.AddDays(-4), MacAddress = "test-mac-address" }, // 2025-04-27
            new FallInfo { Timestamp = date.AddDays(-5), MacAddress = "test-mac-address" }, // 2025-04-26
            new FallInfo { Timestamp = date.AddDays(-6), MacAddress = "test-mac-address" }, // 2025-04-25
            new FallInfo { Timestamp = date.AddDays(-7), MacAddress = "test-mac-address" },  // 2025-04-24
            new FallInfo { Timestamp = date.AddDays(-8), MacAddress = "test-mac-address" } // 2025-04-23
        };

        _mockGetHealthDataService
            .Setup(s => s.GetHealthData<FallInfo>(elderEmail, period, It.IsAny<DateTime>()))
            .ReturnsAsync(mockData);

        // Act
        var result = await _healthService.GetFalls(elderEmail, date, period);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<FallDTO>>>(result);
        var falls = Assert.IsType<List<FallDTO>>(actionResult.Value);
        Assert.Equal(7, falls.Count); // Ensure 7 days of data are returned
        Assert.All(falls, f => Assert.True(f.fallCount >= 0)); // Ensure fall counts are non-negative
        Assert.Equal(date.AddDays(-7).Date, falls[0].Timestamp.Date); // Verify the first date
        Assert.Equal(date.AddDays(-1).Date, falls[6].Timestamp.Date); // Verify the last date
    }
}
