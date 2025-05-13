using HealthDevice.Models;
using HealthDevice.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

public class ArduinoServiceTests
{
    private readonly Mock<ILogger<ArduinoService>> _mockLogger;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IRepository<GPSData>> _mockGpsRepository;
    private readonly Mock<IRepository<Heartrate>> _mockHeartrateRepository;
    private readonly Mock<IRepository<Spo2>> _mockspo2Repository;
    private readonly Mock<IRepository<Steps>> _mockStepsRepository;
    private readonly Mock<IRepository<Arduino>> _mockArduinoRepository;
    private readonly Mock<HttpContext> _mockHttpContext;
    private readonly ArduinoService _arduinoService;

    public ArduinoServiceTests()
    {
        _mockLogger = new Mock<ILogger<ArduinoService>>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockGpsRepository = new Mock<IRepository<GPSData>>();
        _mockHeartrateRepository = new Mock<IRepository<Heartrate>>();
        _mockspo2Repository = new Mock<IRepository<Spo2>>();
        _mockStepsRepository = new Mock<IRepository<Steps>>();
        _mockArduinoRepository = new Mock<IRepository<Arduino>>();
        _mockHttpContext = new Mock<HttpContext>();

        _mockRepositoryFactory.Setup(f => f.GetRepository<GPSData>()).Returns(_mockGpsRepository.Object);
        _mockRepositoryFactory.Setup(f => f.GetRepository<Heartrate>()).Returns(_mockHeartrateRepository.Object);
        _mockRepositoryFactory.Setup(f => f.GetRepository<Spo2>()).Returns(_mockspo2Repository.Object);
        _mockRepositoryFactory.Setup(f => f.GetRepository<Steps>()).Returns(_mockStepsRepository.Object);

#pragma warning disable CS8625
        _arduinoService = new ArduinoService(
            _mockLogger.Object,
            _mockRepositoryFactory.Object,
            null, // ApplicationDbContext is not used in this test, null is fine here
            _mockGpsRepository.Object,
            _mockStepsRepository.Object,
            _mockHeartrateRepository.Object,
            _mockspo2Repository.Object,
            _mockArduinoRepository.Object);
#pragma warning restore CS8625
    }

    [Fact]
    public async Task HandleSensorData_EmptyData_LogsWarning()
    {
        // Arrange
        var emptyData = new List<GPSData>();
        _mockHttpContext.Setup(c => c.Connection.RemoteIpAddress).Returns(System.Net.IPAddress.Parse("127.0.0.1"));

        // Act
        var result = await _arduinoService.HandleSensorData(emptyData, _mockHttpContext.Object);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("data was empty")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }
}