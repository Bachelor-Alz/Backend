using HealthDevice.Data;
using HealthDevice.DTO;
using HealthDevice.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using EntityFrameworkCore.Testing.Moq;
using Microsoft.EntityFrameworkCore;

public class ArduinoServiceTests
{
    private readonly Mock<ILogger<ArduinoService>> _mockLogger;
    private readonly Mock<UserManager<Elder>> _mockUserManager;
    private readonly ApplicationDbContext _mockDbContext;
    private readonly ArduinoService _arduinoService;

    public ArduinoServiceTests()
    {
        _mockLogger = new Mock<ILogger<ArduinoService>>();
        _mockUserManager = GetMockUserManager<Elder>();

        // Create a mock DbContext with seeded empty lists for your DbSets
        _mockDbContext = Create.MockedDbContextFor<ApplicationDbContext>();
        

        _arduinoService = new ArduinoService(_mockLogger.Object, _mockUserManager.Object, _mockDbContext);
    }

    private Mock<UserManager<T>> GetMockUserManager<T>() where T : class
    {
        var store = new Mock<IUserStore<T>>();
        return new Mock<UserManager<T>>(
            store.Object,
            null!,
            Mock.Of<IPasswordHasher<T>>(),
            new List<IUserValidator<T>>(),
            new List<IPasswordValidator<T>>(),
            Mock.Of<ILookupNormalizer>(),
            Mock.Of<IdentityErrorDescriber>(),
            null!,
            Mock.Of<ILogger<UserManager<T>>>()
        );
    }

    [Fact]
    public async Task HandleSensorData_ShouldLogWarning_WhenDataIsEmpty()
    {
        // Arrange
        var emptyData = new List<GPS>();
        var httpContext = new DefaultHttpContext();

        // Act
        var result = await _arduinoService.HandleSensorData(emptyData, httpContext);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("GPS data was empty")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleArduinoData_ShouldLogWarning_WhenElderNotFound()
    {
        var arduinoData = new Arduino
        {
            MacAddress = "arduino1",
            Latitude = 57.0488,
            Longitude = 9.9217,
            steps = 100,
            Max30102 = new List<ArduinoMax>
            {
                new ArduinoMax { heartRate = 80, SpO2 = 95.5f }
            }
        };

        var httpContext = new DefaultHttpContext();

        // Simulate no elder match
        _mockUserManager.Setup(m => m.Users).Returns(_mockDbContext.Elder.AsQueryable());

        await _arduinoService.HandleArduinoData(arduinoData, httpContext);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("No elder found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleArduinoData_ShouldSaveData_WhenElderExists()
    {
        // Arrange
        var elder = new Elder { Arduino = "arduino1", Name = "Test Elder" };
        await _mockDbContext.Elder.AddAsync(elder);
        await _mockDbContext.SaveChangesAsync();

        _mockUserManager.Setup(m => m.Users).Returns(_mockDbContext.Elder);

        var arduinoData = new Arduino
        {
            MacAddress = "arduino1",
            Latitude = 57.0488,
            Longitude = 9.9217,
            steps = 100,
            Max30102 = new List<ArduinoMax>
            {
                new ArduinoMax { heartRate = 80, SpO2 = 95.5f }
            }
        };

        var httpContext = new DefaultHttpContext();

        // Act
        await _arduinoService.HandleArduinoData(arduinoData, httpContext);

        // Assert
        var savedGpsData = _mockDbContext.GPSData.FirstOrDefault();
        Assert.NotNull(savedGpsData);
        Assert.Equal(arduinoData.Latitude, savedGpsData.Latitude);
        Assert.Equal(arduinoData.Longitude, savedGpsData.Longitude);
        Assert.Equal(elder.Arduino, savedGpsData.Address);

        var savedSteps = _mockDbContext.Steps.FirstOrDefault();
        Assert.NotNull(savedSteps);
        Assert.Equal(arduinoData.steps, savedSteps.StepsCount);

        var savedMax30102Data = _mockDbContext.MAX30102Data.FirstOrDefault();
        Assert.NotNull(savedMax30102Data);
        Assert.Equal(arduinoData.Max30102.First().heartRate, savedMax30102Data.Heartrate);
        Assert.Equal(arduinoData.Max30102.First().SpO2, savedMax30102Data.SpO2);
    }

    [Fact]
    public async Task HandleArduinoData_ShouldLogWarning_WhenMax30102DataIsEmpty()
    {
        // Arrange
        var elder = new Elder { Arduino = "arduino1", Name = "Test Elder" };
        await _mockDbContext.Elder.AddAsync(elder);
        await _mockDbContext.SaveChangesAsync();

        _mockUserManager.Setup(m => m.Users).Returns(_mockDbContext.Elder);

        var arduinoData = new Arduino
        {
            MacAddress = "arduino1",
            Latitude = 57.0488,
            Longitude = 9.9217,
            steps = 100,
            Max30102 = new List<ArduinoMax>()
        };

        var httpContext = new DefaultHttpContext();

        // Act
        await _arduinoService.HandleArduinoData(arduinoData, httpContext);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("No Max30102 data found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }
}
