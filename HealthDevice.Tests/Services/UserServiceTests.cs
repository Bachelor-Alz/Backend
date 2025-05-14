using Moq;
using HealthDevice.Services;
using HealthDevice.DTO;
using HealthDevice.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HealthDevice.Tests.Helpers;

public class UserServiceTests
{
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly Mock<UserManager<Elder>> _mockElderManager;
    private readonly Mock<UserManager<Caregiver>> _mockCaregiverManager;
    private readonly Mock<IRepository<Elder>> _mockElderRepository;
    private readonly Mock<IRepository<Caregiver>> _mockCaregiverRepository;
    private readonly Mock<IRepository<GPSData>> _mockGpsRepository;
    private readonly Mock<IRepository<Arduino>> _mockArduinoRepository;

    private readonly Mock<HttpClient> _mockHttpClient;
    private readonly Mock<ILogger<GeoService>> _mockGeoServiceLogger;
    private readonly Mock<GeoService> _mockGeoService;

    private readonly Mock<ITokenService> _mockTokenService;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockLogger = new Mock<ILogger<UserService>>();

        _mockHttpClient = new Mock<HttpClient>();
        _mockGeoServiceLogger = new Mock<ILogger<GeoService>>();
        _mockGeoService = new Mock<GeoService>(_mockHttpClient.Object, _mockGeoServiceLogger.Object);

        _mockElderManager = new Mock<UserManager<Elder>>(
            Mock.Of<IUserStore<Elder>>(),
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<IPasswordHasher<Elder>>(),
            new List<IUserValidator<Elder>>(),
            new List<IPasswordValidator<Elder>>(),
            Mock.Of<ILookupNormalizer>(),
            Mock.Of<IdentityErrorDescriber>(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<Elder>>>()
        );

        _mockCaregiverManager = new Mock<UserManager<Caregiver>>(
            Mock.Of<IUserStore<Caregiver>>(),
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<IPasswordHasher<Caregiver>>(),
            new List<IUserValidator<Caregiver>>(),
            new List<IPasswordValidator<Caregiver>>(),
            Mock.Of<ILookupNormalizer>(),
            Mock.Of<IdentityErrorDescriber>(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<Caregiver>>>()
        );

        _mockElderRepository = new Mock<IRepository<Elder>>();
        _mockCaregiverRepository = new Mock<IRepository<Caregiver>>();
        _mockGpsRepository = new Mock<IRepository<GPSData>>();
        _mockArduinoRepository = new Mock<IRepository<Arduino>>();
        _mockTokenService = new Mock<ITokenService>();

        _mockTokenService.Setup(ts => ts.GenerateAccessToken(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .Returns("mock-access-token");

        _mockTokenService.Setup(ts => ts.IssueRefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new RefreshTokenResult { Token = "mock-refresh-token" });

        _mockTokenService.Setup(ts => ts.ValidateRefreshTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        _userService = new UserService(
            _mockLogger.Object,
            _mockElderManager.Object,
            _mockCaregiverManager.Object,
            _mockElderRepository.Object,
            _mockCaregiverRepository.Object,
            _mockGpsRepository.Object,
            _mockGeoService.Object,
            _mockTokenService.Object, 
            _mockArduinoRepository.Object
        );
    }

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
    public async Task HandleLogin_ShouldReturnToken_WhenLoginIsSuccessful()
    {
        // Arrange
        var userLoginDto = new UserLoginDTO { Email = "elder@test.com", Password = "Password123!" };
        var ipAddress = "127.0.0.1";
        var elder = new Elder { Email = userLoginDto.Email, Name = "Test Elder" };

        _mockElderRepository.Setup(repo => repo.Query())
            .Returns(CreateMockQueryable(new List<Elder> { elder }));

        _mockElderManager.Setup(manager => manager.CheckPasswordAsync(elder, userLoginDto.Password))
            .ReturnsAsync(true);

        _mockElderManager.Setup(manager => manager.FindByEmailAsync(userLoginDto.Email))
            .ReturnsAsync(elder);

        // Act
        var result = await _userService.HandleLogin(userLoginDto, ipAddress);

        // Assert
        var actionResult = Assert.IsType<ActionResult<LoginResponseDTO>>(result);
        var loginResponse = Assert.IsType<LoginResponseDTO>(actionResult.Value);
        Assert.NotNull(loginResponse.Token);
        Assert.Equal("mock-access-token", loginResponse.Token);
        Assert.Equal("mock-refresh-token", loginResponse.RefreshToken);
        Assert.Equal(Roles.Elder, loginResponse.Role);
    }

    [Fact]
    public async Task HandleLogin_InvalidElderPassword_ReturnsUnauthorized()
    {
        // Arrange
        var userLoginDto = new UserLoginDTO { Email = "elder@test.com", Password = "WrongPassword" };
        var ipAddress = "127.0.0.1";
        var elder = new Elder { Email = userLoginDto.Email, Name = "Test Elder" };

        _mockElderRepository.Setup(repo => repo.Query())
            .Returns(CreateMockQueryable(new List<Elder> { elder }));

        _mockElderManager.Setup(manager => manager.CheckPasswordAsync(elder, userLoginDto.Password))
            .ReturnsAsync(false);

        // Act
        var result = await _userService.HandleLogin(userLoginDto, ipAddress);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task HandleLogin_UserNotFound_ReturnsUnauthorized()
    {
        // Arrange
        var userLoginDto = new UserLoginDTO { Email = "elder@test.com", Password = "Password123!" };
        var ipAddress = "127.0.0.1";

        _mockElderRepository.Setup(repo => repo.Query())
            .Returns(CreateMockQueryable(new List<Elder>()));

        _mockCaregiverRepository.Setup(repo => repo.Query())
            .Returns(CreateMockQueryable(new List<Caregiver>()));

        // Act
        var result = await _userService.HandleLogin(userLoginDto, ipAddress);

        // Assert
        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task HandleRegister_SuccessfulRegistration_ReturnsOk()
    {
        // Arrange
        var userManager = _mockElderManager.Object;
        var userRegisterDto = new UserRegisterDTO { Email = "elder@test.com", Password = "Password123!", Name = "Test Elder", Role = Roles.Elder };
        var newUser = new Elder { Email = userRegisterDto.Email, Name = userRegisterDto.Name };
        var ipAddress = "127.0.0.1";

        _mockElderManager.Setup(manager => manager.Users)
            .Returns(CreateMockQueryable(new List<Elder>()));

        _mockElderManager.Setup(manager => manager.CreateAsync(newUser, userRegisterDto.Password))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _userService.HandleRegister(userManager, userRegisterDto, newUser, ipAddress);


        // Assert
        Assert.IsType<OkObjectResult>(result);
        _mockElderManager.Verify(manager => manager.CreateAsync(newUser, userRegisterDto.Password), Times.Once);
    }

    [Fact]
    public async Task HandleRegister_EmailAlreadyExists_ReturnsBadRequest()
    {
        // Arrange
        var userManager = _mockElderManager.Object;
        var userRegisterDto = new UserRegisterDTO { Email = "elder@test.com", Password = "Password123!", Name = "Test Elder", Role = Roles.Elder };
        var existingUser = new Elder { Email = userRegisterDto.Email, Name = "Test Elder" };
        var newUser = new Elder { Email = userRegisterDto.Email, Name = userRegisterDto.Name };
        var ipAddress = "127.0.0.1";

        _mockElderManager.Setup(manager => manager.Users)
            .Returns(CreateMockQueryable(new List<Elder> { existingUser }));

        // Act
        var result = await _userService.HandleRegister(userManager, userRegisterDto, newUser, ipAddress);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Email already exists.", badRequestResult.Value);

        _mockElderManager.Verify(manager => manager.CreateAsync(It.IsAny<Elder>(), It.IsAny<string>()), Times.Never);
    }
}

