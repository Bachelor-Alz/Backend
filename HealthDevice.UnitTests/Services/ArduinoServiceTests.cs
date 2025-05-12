using HealthDevice.DTO;
using HealthDevice.Data;
using HealthDevice.Models;
using HealthDevice.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class ArduinoServiceTests
{
    private readonly Mock<ILogger<ArduinoService>> _mockLogger;
    private readonly Mock<IRepositoryFactory> _mockRepositoryFactory;
    private readonly Mock<IRepository<GPSData>> _mockGpsRepository;
    private readonly Mock<IRepository<Max30102>> _mockMax30102Repository;
    private readonly Mock<IRepository<Steps>> _mockStepsRepository;
    private readonly Mock<HttpContext> _mockHttpContext;
    private readonly ArduinoService _arduinoService;

    public ArduinoServiceTests()
    {
        _mockLogger = new Mock<ILogger<ArduinoService>>();
        _mockRepositoryFactory = new Mock<IRepositoryFactory>();
        _mockGpsRepository = new Mock<IRepository<GPSData>>();
        _mockMax30102Repository = new Mock<IRepository<Max30102>>();
        _mockStepsRepository = new Mock<IRepository<Steps>>();
        _mockHttpContext = new Mock<HttpContext>();

        _mockRepositoryFactory.Setup(f => f.GetRepository<GPSData>()).Returns(_mockGpsRepository.Object);
        _mockRepositoryFactory.Setup(f => f.GetRepository<Max30102>()).Returns(_mockMax30102Repository.Object);
        _mockRepositoryFactory.Setup(f => f.GetRepository<Steps>()).Returns(_mockStepsRepository.Object);

        #pragma warning disable CS8625
        _arduinoService = new ArduinoService(
            _mockLogger.Object,
            _mockRepositoryFactory.Object,
            null, // ApplicationDbContext is not used in this test, null is fine here
            _mockGpsRepository.Object,
            _mockMax30102Repository.Object,
            _mockStepsRepository.Object
);

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