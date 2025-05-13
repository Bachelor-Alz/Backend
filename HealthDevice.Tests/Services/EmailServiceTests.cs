using HealthDevice.Models;
using HealthDevice.Services;
using Microsoft.Extensions.Logging;
using Moq;
using HealthDevice.Tests.Helpers;

public class EmailServiceTests
{
    private readonly Mock<ILogger<EmailService>> _mockLogger;
    private readonly Mock<IRepository<Caregiver>> _mockCaregiverRepository;
    private readonly EmailService _emailService;

    public EmailServiceTests()
    {
        // Set SMTP configuration before EmailService is created
        Environment.SetEnvironmentVariable("SMTP_HOST", "smtp.test.com");
        Environment.SetEnvironmentVariable("SMTP_PORT", "587");
        Environment.SetEnvironmentVariable("SMTP_USER", "test-smtp-user");
        Environment.SetEnvironmentVariable("SMTP_PASSWORD", "test-smtp-password");

        _mockLogger = new Mock<ILogger<EmailService>>();
        _mockCaregiverRepository = new Mock<IRepository<Caregiver>>();

        _emailService = new EmailService(
            _mockLogger.Object,
            _mockCaregiverRepository.Object
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
    public async Task SendEmail_InvalidConfiguration_LogsWarning()
    {
        // Arrange: clear SMTP configuration (simulate invalid setup)
        Environment.SetEnvironmentVariable("SMTP_HOST", "");
        Environment.SetEnvironmentVariable("SMTP_PORT", "");
        Environment.SetEnvironmentVariable("SMTP_USER", "");
        Environment.SetEnvironmentVariable("SMTP_PASSWORD", "");

        var loggerMock = new Mock<ILogger<EmailService>>();
        var repoMock = new Mock<IRepository<Caregiver>>();

        // Recreate the service with invalid config
        var emailService = new EmailService(loggerMock.Object, repoMock.Object);

        var elder = new Elder { Name = "Test Elder", Email = "Test@Elder.com" };

        // Act
        await emailService.SendEmail("Subject", "Body", elder);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SMTP configuration is not set")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task SendEmail_NoCaregiversAssociated_LogsWarning()
    {
        // Arrange
        var elder = new Elder { Name = "Test Elder", Email = "elder@test.com" };

        // Mock caregiver repository to return no caregivers
        _mockCaregiverRepository.Setup(r => r.Query())
            .Returns(CreateMockQueryable(new List<Caregiver>()));

        // Act
        await _emailService.SendEmail("Test Subject", "Test Body", elder);

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No caregivers found for elder")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }
}