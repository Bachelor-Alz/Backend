using HealthDevice.DTO;
using HealthDevice.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using HealthDevice.UnitTests.Helpers;

public class UserServiceTests
{
    private Mock<UserManager<T>> GetMockUserManager<T>() where T : class
    {
        var store = new Mock<IUserStore<T>>();
        var mockUserManager = new Mock<UserManager<T>>(
            store.Object,
            null!, // OptionsAccessor
            Mock.Of<IPasswordHasher<T>>(),
            new List<IUserValidator<T>>(),
            new List<IPasswordValidator<T>>(),
            Mock.Of<ILookupNormalizer>(),
            Mock.Of<IdentityErrorDescriber>(),
            null!, // Services
            Mock.Of<ILogger<UserManager<T>>>()
        );

        return mockUserManager;
    }

    [Fact]
    public async Task HandleLogin_ShouldReturnToken_WhenLoginIsSuccessful()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UserService>>();
        var mockElderManager = GetMockUserManager<Elder>();
        var mockCaregiverManager = GetMockUserManager<Caregiver>();
        var mockRepositoryFactory = new Mock<IRepositoryFactory>();

        var elder = new Elder { Email = "test@example.com", UserName = "test@example.com", Name = "Test Elder" };

        // Mock Elder repository with async support
        var elders = new List<Elder> { elder }.AsQueryable();
        var asyncElders = new TestDbAsyncEnumerable<Elder>(elders);
        var elderRepository = new Mock<IRepository<Elder>>();
        elderRepository.Setup(r => r.Query()).Returns(asyncElders);
        mockRepositoryFactory.Setup(f => f.GetRepository<Elder>()).Returns(elderRepository.Object);

        // Mock UserManager methods
        mockElderManager.Setup(m => m.FindByEmailAsync("test@example.com")).ReturnsAsync(elder);
        mockElderManager.Setup(m => m.CheckPasswordAsync(elder, "Password123!")).ReturnsAsync(true);

        var userService = new UserService(
            mockLogger.Object,
            mockElderManager.Object,
            mockCaregiverManager.Object,
            mockRepositoryFactory.Object
        );

        var loginDto = new UserLoginDTO { Email = "test@example.com", Password = "Password123!" };

        // Initialize HttpContext
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

        // Act
        var result = await userService.HandleLogin(loginDto, httpContext);

        // Assert
        var actionResult = Assert.IsType<ActionResult<LoginResponseDTO>>(result);
        var loginResponse = Assert.IsType<LoginResponseDTO>(actionResult.Value);
        Assert.NotNull(loginResponse.Token);
        Assert.Equal(Roles.Elder, loginResponse.role);
    }

    [Fact]
    public async Task HandleRegister_ShouldReturnOk_WhenRegistrationIsSuccessful()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UserService>>();
        var mockElderManager = GetMockUserManager<Elder>();
        var mockCaregiverManager = GetMockUserManager<Caregiver>();
        var mockRepositoryFactory = new Mock<IRepositoryFactory>();

        var elderRepository = new Mock<IRepository<Elder>>();
        mockRepositoryFactory.Setup(f => f.GetRepository<Elder>()).Returns(elderRepository.Object);

        var elder = new Elder { Email = "test@example.com", UserName = "test@example.com", Name = "Test Elder" };
        mockElderManager.Setup(m => m.FindByEmailAsync("test@example.com")).ReturnsAsync((Elder?)null);
        mockElderManager.Setup(m => m.CreateAsync(It.IsAny<Elder>(), "password")).ReturnsAsync(IdentityResult.Success);

        var userService = new UserService(mockLogger.Object, mockElderManager.Object, mockCaregiverManager.Object, mockRepositoryFactory.Object);

        var registerDto = new UserRegisterDTO
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "password",
            Role = Roles.Elder
        };
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

        // Act
        var result = await userService.HandleRegister(mockElderManager.Object, registerDto, elder, httpContext);

        // Assert
        var actionResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Registration successful.", actionResult.Value);
    }

    [Fact]
    public async Task HandleRegister_ShouldReturnBadRequest_WhenEmailAlreadyExists()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UserService>>();
        var mockElderManager = GetMockUserManager<Elder>();
        var mockCaregiverManager = GetMockUserManager<Caregiver>();
        var mockRepositoryFactory = new Mock<IRepositoryFactory>();

        var elder = new Elder { Email = "test@example.com", UserName = "test@example.com", Name = "Test Elder" };

        var elderRepository = new Mock<IRepository<Elder>>();
        mockRepositoryFactory.Setup(f => f.GetRepository<Elder>()).Returns(elderRepository.Object);

        var elders = new List<Elder> { elder }.AsQueryable();
        var asyncElders = new TestDbAsyncEnumerable<Elder>(elders);
        elderRepository.Setup(r => r.Query()).Returns(asyncElders);

        var userService = new UserService(mockLogger.Object, mockElderManager.Object, mockCaregiverManager.Object, mockRepositoryFactory.Object);

        var registerDto = new UserRegisterDTO
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "password",
            Role = Roles.Elder
        };
        var httpContext = new DefaultHttpContext();

        // Act
        var result = await userService.HandleRegister(mockElderManager.Object, registerDto, elder, httpContext);

        // Assert
        var actionResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Email already exists.", actionResult.Value);
    }
}