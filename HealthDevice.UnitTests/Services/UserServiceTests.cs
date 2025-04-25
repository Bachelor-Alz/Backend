using HealthDevice.DTO;
using HealthDevice.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

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

        var userService = new UserService(mockLogger.Object, mockElderManager.Object, mockCaregiverManager.Object);

        var elder = new Elder { Email = "test@example.com", UserName = "test@example.com", Name = "Test Elder" };
        mockElderManager.Setup(m => m.FindByEmailAsync("test@example.com")).ReturnsAsync(elder);
        mockElderManager.Setup(m => m.CheckPasswordAsync(elder, "password")).ReturnsAsync(true);

        var loginDto = new UserLoginDTO { Email = "test@example.com", Password = "password" };
        var httpContext = new DefaultHttpContext();

        // Act
        var result = await userService.HandleLogin(loginDto, httpContext);

        // Assert
        var actionResult = Assert.IsType<ActionResult<LoginResponseDTO>>(result);
        var loginResponse = Assert.IsType<LoginResponseDTO>(actionResult.Value);
        Assert.NotNull(loginResponse.Token);
        Assert.Equal(Roles.Elder, loginResponse.role);
    }

    [Fact]
    public async Task HandleLogin_ShouldReturnUnauthorized_WhenPasswordIsInvalid()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UserService>>();
        var mockElderManager = GetMockUserManager<Elder>();
        var mockCaregiverManager = GetMockUserManager<Caregiver>();

        var userService = new UserService(mockLogger.Object, mockElderManager.Object, mockCaregiverManager.Object);

        var elder = new Elder { Email = "test@example.com", UserName = "test@example.com", Name = "Test Elder" };
        mockElderManager.Setup(m => m.FindByEmailAsync("test@example.com")).ReturnsAsync(elder);
        mockElderManager.Setup(m => m.CheckPasswordAsync(elder, "wrongpassword")).ReturnsAsync(false);

        var loginDto = new UserLoginDTO { Email = "test@example.com", Password = "wrongpassword" };
        var httpContext = new DefaultHttpContext();

        // Act
        var result = await userService.HandleLogin(loginDto, httpContext);

        // Assert
        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task HandleRegister_ShouldReturnOk_WhenRegistrationIsSuccessful()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UserService>>();
        var mockElderManager = GetMockUserManager<Elder>();
        var mockCaregiverManager = GetMockUserManager<Caregiver>();

        var elder = new Elder { Email = "test@example.com", UserName = "test@example.com", Name = "Test Elder" };
        mockElderManager.Setup(m => m.FindByEmailAsync("test@example.com")).ReturnsAsync((Elder?)null);
        mockElderManager.Setup(m => m.CreateAsync(It.IsAny<Elder>(), "password")).ReturnsAsync(IdentityResult.Success);

        var userService = new UserService(mockLogger.Object, mockElderManager.Object, mockCaregiverManager.Object);

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

        var userService = new UserService(mockLogger.Object, mockElderManager.Object, mockCaregiverManager.Object);

        var elder = new Elder { Email = "test@example.com", UserName = "test@example.com", Name = "Test Elder" };
        mockElderManager.Setup(m => m.FindByEmailAsync("test@example.com")).ReturnsAsync(elder);

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