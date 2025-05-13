using HealthDevice.Models;
using HealthDevice.Services;
using Microsoft.Extensions.Logging;
using Moq;
using HealthDevice.Tests.Helpers;

public class AIServiceTests
{
    private readonly Mock<ILogger<AiService>> _mockLogger;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IGeoService> _mockGeoService;
    private readonly Mock<IRepository<Elder>> _mockElderRepository;
    private readonly Mock<IRepository<Caregiver>> _mockCaregiverRepository;
    private readonly Mock<IRepository<FallInfo>> _mockFallInfoRepository;
    private readonly Mock<IRepository<Location>> _mockLocationRepository;
    private readonly AiService _aiService;

    public AIServiceTests()
    {
        _mockLogger = new Mock<ILogger<AiService>>();
        _mockEmailService = new Mock<IEmailService>();
        _mockGeoService = new Mock<IGeoService>();
        _mockElderRepository = new Mock<IRepository<Elder>>();
        _mockCaregiverRepository = new Mock<IRepository<Caregiver>>();
        _mockFallInfoRepository = new Mock<IRepository<FallInfo>>();
        _mockLocationRepository = new Mock<IRepository<Location>>();

        _aiService = new AiService(
            _mockLogger.Object,
            _mockEmailService.Object,
            _mockGeoService.Object,
            _mockElderRepository.Object,
            _mockCaregiverRepository.Object,
            _mockFallInfoRepository.Object,
            _mockLocationRepository.Object
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
    public async Task HandleAiRequest_FallDetected_LogsAndHandlesFall()
    {
        // Arrange
        var request = new List<int> { 1, 1, 1, 1 }; // Simulates a fall
        var macAddress = "test-mac-address";
        var elder = new Elder { Name = "Test Elder", Email = "elder@test.com", MacAddress = macAddress };

        _mockLogger.Setup(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Fall detected for elder {macAddress}")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ));

        // Act
        await _aiService.HandleAiRequest(request, macAddress);

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Fall detected for elder {macAddress}")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAiRequest_NoFallDetected_LogsNoFall()
    {
        // Arrange
        var request = new List<int> { 0, 0, 1, 0 }; // No fall detected
        var macAddress = "test-mac-address";

        // Act
        await _aiService.HandleAiRequest(request, macAddress);

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"No fall detected for elder {macAddress}")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once
        );
    }
}