using HealthDevice.Models;
using HealthDevice.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.AspNetCore.Mvc;
using HealthDevice.UnitTests.Helpers;

public class HealthServiceTests
{
    private readonly Mock<ILogger<HealthService>> _mockLogger;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IGetHealthData> _mockGetHealthDataService;
    private readonly Mock<ITimeZoneService> _mockTimeZoneService;
    private readonly Mock<IRepository<Elder>> _mockElderRepository;
    private readonly Mock<IRepository<GPSData>> _mockGpsRepository;
    private readonly Mock<IRepository<Perimeter>> _mockPerimeterRepository;
    private readonly Mock<IRepository<Location>> _mockLocationRepository;
    private readonly Mock<IRepository<Max30102>> _mockMax30102Repository;
    private readonly Mock<IRepository<Steps>> _mockStepsRepository;
    private readonly Mock<IRepository<DistanceInfo>> _mockDistanceInfoRepository;
    private readonly Mock<IRepository<FallInfo>> _mockFallInfoRepository;
    private readonly HealthService _healthService;

    public HealthServiceTests()
    {
        _mockLogger = new Mock<ILogger<HealthService>>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockEmailService = new Mock<IEmailService>();
        _mockGetHealthDataService = new Mock<IGetHealthData>();
        _mockTimeZoneService = new Mock<ITimeZoneService>();
        _mockElderRepository = new Mock<IRepository<Elder>>();
        _mockGpsRepository = new Mock<IRepository<GPSData>>();
        _mockPerimeterRepository = new Mock<IRepository<Perimeter>>();
        _mockLocationRepository = new Mock<IRepository<Location>>();
        _mockMax30102Repository = new Mock<IRepository<Max30102>>();
        _mockStepsRepository = new Mock<IRepository<Steps>>();
        _mockDistanceInfoRepository = new Mock<IRepository<DistanceInfo>>();
        _mockFallInfoRepository = new Mock<IRepository<FallInfo>>();


        _healthService = new HealthService(
            _mockLogger.Object,
            _mockRepositoryFactory.Object,
            _mockEmailService.Object,
            _mockGetHealthDataService.Object,
            _mockTimeZoneService.Object,
            _mockElderRepository.Object,
            Mock.Of<IRepository<Caregiver>>(), // Caregiver repository is not used in the methods being tested
            _mockPerimeterRepository.Object,
            _mockLocationRepository.Object,
            _mockMax30102Repository.Object,
            _mockStepsRepository.Object,
            _mockDistanceInfoRepository.Object,
            _mockFallInfoRepository.Object
        );
    }

    // Helper method to create an IQueryable mock using helpers
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
           .Returns(((IAsyncEnumerable<T>)new TestDbAsyncEnumerable<T>(data)).GetAsyncEnumerator());

        return mock.Object;
    }


    [Fact]
    public async Task DeleteGpsData_NoDataFound_LogsWarning()
    {
        // Arrange
        _mockGpsRepository.Setup(r => r.Query()).Returns(CreateMockQueryable(new List<GPSData>()));
        _mockRepositoryFactory.Setup(f => f.GetRepository<GPSData>()).Returns(_mockGpsRepository.Object);

        // Act
        await _healthService.DeleteGpsData(DateTime.UtcNow, "MacAddress");

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No GPS data found to delete for elder MacAddress")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteGpsData_DataFound_DeletesData()
    {
        // Arrange
        var gpsData = new List<GPSData>
        {
            new GPSData { Timestamp = DateTime.UtcNow.AddDays(-1), MacAddress = "test-mac-address" }
        };
        _mockGpsRepository.Setup(r => r.Query()).Returns(CreateMockQueryable(gpsData));
        _mockRepositoryFactory.Setup(f => f.GetRepository<GPSData>()).Returns(_mockGpsRepository.Object);

        // Act
        await _healthService.DeleteGpsData(DateTime.UtcNow, "test-mac-address");

        // Assert
        _mockGpsRepository.Verify(r => r.RemoveRange(It.IsAny<List<GPSData>>()), Times.Once);
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Deleted 1 GPS records for elder test-mac-address")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task SetPerimeter_InvalidRadius_ReturnsBadRequest()
    {
        // Arrange
        _mockElderRepository.Setup(r => r.Query()).Returns(CreateMockQueryable(new List<Elder>()));

        // Act
        var result = await _healthService.SetPerimeter(-1, "elder@test.com");

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid radius value.", badRequest.Value);
    }

    [Fact]
    public async Task SetPerimeter_ElderNotFound_ReturnsBadRequest()
    {
        // Arrange
        _mockElderRepository.Setup(r => r.Query()).Returns(CreateMockQueryable(new List<Elder>()));

        // Act
        var result = await _healthService.SetPerimeter(10, "elder@test.com");

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Elder Arduino not set.", badRequest.Value);
    }

    [Fact]
    public async Task ComputeOutOfPerimeter_ShouldUpdateElderLocation_WhenElderIsOutOfPerimeter()
    {
        // Arrange
        var macAddress = "test-mac-address";
        var location = new Location { Latitude = 56.0124, Longitude = 9.9915, Timestamp = DateTime.UtcNow };
        var perimeter = new Perimeter { Latitude = 57.0000, Longitude = 10.0000, Radius = 5, MacAddress = macAddress };
        var elder = new Elder { Name = "Test Elder", Email = "test@elder.com", MacAddress = macAddress, OutOfPerimeter = true };

        _mockPerimeterRepository.Setup(r => r.Query()).Returns(CreateMockQueryable([perimeter]));
        _mockElderRepository.Setup(r => r.Query()).Returns(CreateMockQueryable([elder]));
        _mockElderRepository.Setup(r => r.Update(It.IsAny<Elder>())).Returns(Task.CompletedTask);


        // Act
        await _healthService.ComputeOutOfPerimeter(macAddress, location);

        // Assert
        Assert.True(elder.OutOfPerimeter, "Elder should remain out of perimeter.");
        _mockElderRepository.Verify(r => r.Update(It.IsAny<Elder>()), Times.Never);
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Elder {elder.Email} is already out of perimeter")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ComputeOutOfPerimeter_ShouldUpdateElderLocation_WhenElderIsInPerimeter()
    {
        // Arrange
        var macAddress = "test-mac-address";
        var location = new Location { Latitude = 57.0010, Longitude = 10.0010, Timestamp = DateTime.UtcNow };
        var perimeter = new Perimeter { Latitude = 57.0000, Longitude = 10.0000, Radius = 5, MacAddress = macAddress };
        var elder = new Elder { Name = "Test Elder", Email = "test@elder.com", MacAddress = macAddress, OutOfPerimeter = true };

        _mockPerimeterRepository.Setup(r => r.Query()).Returns(CreateMockQueryable([perimeter]));
        _mockElderRepository.Setup(r => r.Query()).Returns(CreateMockQueryable([elder]));

        // Act
        await _healthService.ComputeOutOfPerimeter(macAddress, location);

        // Assert
        Assert.False(elder.OutOfPerimeter, "Elder should be marked as in the perimeter.");
        _mockElderRepository.Verify(r => r.Update(It.Is<Elder>(e => e.MacAddress == macAddress && e.OutOfPerimeter == false)), Times.Once);
    }

    [Fact]
    public async Task ComputeOutOfPerimeter_ShouldUpdateElderLocation_WhenElderWasInPerimeterAndGoesOut()
    {
        // Arrange
        var macAddress = "test-mac-address";

        var location = new Location { Latitude = 56.0124, Longitude = 9.9915, Timestamp = DateTime.UtcNow };
        var perimeter = new Perimeter { Latitude = 57.0000, Longitude = 10.0000, Radius = 5, MacAddress = macAddress };
        var elder = new Elder { Name = "Test Elder", Email = "test@example.com", MacAddress = macAddress, OutOfPerimeter = false };

        var mockPerimeterRepository = new Mock<IRepository<Perimeter>>();
        var mockElderRepository = new Mock<IRepository<Elder>>();

        _mockPerimeterRepository.Setup(r => r.Query()).Returns(CreateMockQueryable([perimeter]));
        _mockElderRepository.Setup(r => r.Query()).Returns(CreateMockQueryable([elder]));
        _mockElderRepository.Setup(r => r.Update(It.IsAny<Elder>())).Returns(Task.CompletedTask);


        // Act
        await _healthService.ComputeOutOfPerimeter(macAddress, location);

        // Assert
        Assert.True(elder.OutOfPerimeter, "Elder should be marked as out of perimeter.");
        _mockElderRepository.Verify(r => r.Update(It.Is<Elder>(e => e.MacAddress == macAddress && e.OutOfPerimeter == true)), Times.Once);
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Elder {elder.Email} is out of perimeter")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GetLocation_ShouldReturnElderLocation_WhenGPSExists()
    {
        // Arrange
        var macAddress = "test-mac-address";
        var currentTime = DateTime.UtcNow;

        var gpsData = new GPSData
        {
            Latitude = 57.0124,
            Longitude = 9.9915,
            Timestamp = currentTime.AddMinutes(-5),
            MacAddress = macAddress
        };

        _mockGpsRepository.Setup(r => r.Query()).Returns(CreateMockQueryable([gpsData]));
        _mockRepositoryFactory.Setup(f => f.GetRepository<GPSData>()).Returns(_mockGpsRepository.Object);

        // Act
        var result = await _healthService.GetLocation(currentTime, macAddress);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Location>(result);
        Assert.Equal(gpsData.Latitude, result.Latitude);
        Assert.Equal(gpsData.Longitude, result.Longitude);
        Assert.Equal(gpsData.Timestamp, result.Timestamp);
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"GPS data found for elder {macAddress}")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }


    [Fact]
    public async Task GetLocation_ShouldReturnDefaultLocation_WhenGPSDataIsNull()
    {
        // Arrange
        var macAddress = "test-mac-address";
        var currentTime = DateTime.UtcNow;

        _mockGpsRepository.Setup(r => r.Query()).Returns(CreateMockQueryable(new List<GPSData>()));
        _mockRepositoryFactory.Setup(f => f.GetRepository<GPSData>()).Returns(_mockGpsRepository.Object);

        // Act
        var result = await _healthService.GetLocation(currentTime, macAddress);

        // Assert
        Assert.NotNull(result);
        var locationResult = Assert.IsType<Location>(result);
        Assert.Equal(0, locationResult.Latitude);
        Assert.Equal(0, locationResult.Longitude);
        Assert.Equal(default, locationResult.Timestamp);

        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"No GPS data found for elder {macAddress}")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }
}
