using HealthDevice.Data;
using HealthDevice.DTO;
using HealthDevice.Models;
using HealthDevice.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.AspNetCore.Mvc;

namespace HealthDevice.Tests.Services;

public class IntegrationAttempt
{
    private readonly ApplicationDbContext _dbContext;
    private readonly HealthService _healthService;

    public IntegrationAttempt()
    {
        // Set up the in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("HealthDevice_Test")
            .Options;

        _dbContext = new ApplicationDbContext(options);

        SeedDatabase();

        // Mock dependencies for HealthService
        var mockLogger = new Mock<ILogger<HealthService>>();
        var mockRepositoryFactory = new Mock<IRepositoryFactory>();
        var mockEmailService = new Mock<IEmailService>();
        var mockGetHealthDataService = new Mock<IGetHealthData>();
        var mockTimeZoneService = new Mock<ITimeZoneService>();

        // Set up repository factory to return in-memory repositories
        mockRepositoryFactory.Setup(f => f.GetRepository<Sensor>())
            .Returns(new Repository<Sensor>(_dbContext));
        mockRepositoryFactory.Setup(f => f.GetRepository<GPSData>())
            .Returns(new Repository<GPSData>(_dbContext));

        // Initialize the HealthService
        _healthService = new HealthService(
            mockLogger.Object,
            mockRepositoryFactory.Object,
            mockEmailService.Object,
            mockGetHealthDataService.Object,
            mockTimeZoneService.Object,
            new Repository<Elder>(_dbContext),
            new Repository<Caregiver>(_dbContext),
            new Repository<Perimeter>(_dbContext),
            new Repository<Location>(_dbContext),
            new Repository<Steps>(_dbContext),
            new Repository<DistanceInfo>(_dbContext),
            new Repository<FallInfo>(_dbContext),
            new Repository<Heartrate>(_dbContext),
            new Repository<Spo2>(_dbContext)
        );
    }

    private void SeedDatabase()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();
        // Add test data for the elder
        var elder = new Elder
        {
            Id = "test-elder-id",
            Name = "Test Elder",
            Email = "test@elder.com",
            Latitude = 55.6761,
            Longitude = 12.5683,
            MacAddress = "test-mac-address"
        };

        var location = new Location
        {
            Latitude = 55.6770,
            Longitude = 12.5690,
            Timestamp = DateTime.UtcNow,
            MacAddress = "test-mac-address"
        };

        _dbContext.Location.Add(location);
        _dbContext.Elder.Add(elder);

        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetElderPerimeter_ShouldReturnCorrectPerimeter()
    {
        // Arrange
        var elderId = "test-elder-id";

        // Act
        var result = await _healthService.GetElderPerimeter(elderId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PerimeterDTO>>(result);
        Assert.NotNull(actionResult.Value);

        // Assert PerimeterDTO properties
        Assert.Equal(55.6761, actionResult.Value.HomeLatitude);
        Assert.Equal(12.5683, actionResult.Value.HomeLongitude);
        Assert.Equal(10, actionResult.Value.HomeRadius); // SHOULD DEFAULT TO 10 
    }
}