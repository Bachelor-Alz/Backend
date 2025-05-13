using HealthDevice.DTO;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HealthDevice.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
// ReSharper disable SuggestVarOrType_SimpleTypes
namespace HealthDevice.Services;


public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly UserManager<Elder> _elderManager;
    private readonly UserManager<Caregiver> _caregiverManager;
    private readonly IRepository<Elder> _elderRepository;
    private readonly IRepository<Caregiver> _caregiverRepository;
    private readonly IRepository<GPSData> _gpsRepository;
    private readonly GeoService _geoService;
    private readonly TokenService _tokenService;

    public UserService(ILogger<UserService> logger, UserManager<Elder> elderManager,
        UserManager<Caregiver> caregiverManager, IRepository<Elder> elderRepository,
        IRepository<Caregiver> caregiverRepository, IRepository<GPSData> gpsRepository,
        GeoService geoService, TokenService tokenService)
    {
        _logger = logger;
        _elderManager = elderManager;
        _caregiverManager = caregiverManager;
        _elderRepository = elderRepository;
        _caregiverRepository = caregiverRepository;
        _gpsRepository = gpsRepository;
        _geoService = geoService;
        _tokenService = tokenService;
    }

    public async Task<ActionResult<LoginResponseDTO>> HandleLogin(UserLoginDTO userLoginDto, string ipAdress)
    {
        DateTime timestamp = DateTime.UtcNow;
        Elder? elder = await _elderRepository.Query().FirstOrDefaultAsync(m => m.Email != null && m.Email.ToLower() == userLoginDto.Email.ToLower());
        if (elder != null && elder.Email != null)
        {

            if (!await _elderManager.CheckPasswordAsync(elder, userLoginDto.Password))
            {
                _logger.LogWarning("Login failed for Email: {Email} from IP: {IpAddress} at {Timestamp}.", userLoginDto.Email, ipAdress, timestamp);
                return new UnauthorizedObjectResult("Wrong password.");
            }
            RefreshTokenResult refreshTokenResult = await _tokenService.IssueRefreshTokenAsync(elder.Email, ipAdress);
            _logger.LogInformation("Login successful for Email: {Email} from IP: {IpAddress} at {Timestamp}.", userLoginDto.Email, ipAdress, timestamp);
            return new LoginResponseDTO { Token = _tokenService.GenerateAccessToken(elder, "Elder"), Role = Roles.Elder, RefreshToken = refreshTokenResult.Token };
        }

        Caregiver? caregiver = await _caregiverRepository.Query().FirstOrDefaultAsync(m => m.Email != null && m.Email.ToLower() == userLoginDto.Email.ToLower());

        if (caregiver != null && caregiver.Email != null)
        {


            if (!await _caregiverManager.CheckPasswordAsync(caregiver, userLoginDto.Password))
            {
                _logger.LogWarning("Login failed for Email: {Email} from IP: {IpAddress} at {Timestamp}.", userLoginDto.Email, ipAdress, timestamp);
                return new UnauthorizedObjectResult("Wrong password.");
            }
            _logger.LogInformation("Login successful for Email: {Email} from IP: {IpAddress} at {Timestamp}.", userLoginDto.Email, ipAdress, timestamp);
            RefreshTokenResult refreshTokenResult = await _tokenService.IssueRefreshTokenAsync(caregiver.Email, ipAdress);
            return new LoginResponseDTO { Token = _tokenService.GenerateAccessToken(caregiver, "Caregiver"), Role = Roles.Caregiver, RefreshToken = refreshTokenResult.Token };
        }

        _logger.LogInformation("Couldnt find a user with the Email {Email} from IP: {IpAddress} at {Timestamp}.", userLoginDto.Email, ipAdress, timestamp);
        return new UnauthorizedResult();
    }

    public async Task<ActionResult> HandleRegister<T>(UserManager<T> userManager, UserRegisterDTO userRegisterDto, T user, string ipAddress) where T : IdentityUser
    {
        DateTime timestamp = DateTime.UtcNow;

        if (await userManager.Users.FirstOrDefaultAsync(m => m.Email != null && m.Email.ToLower() == userRegisterDto.Email.ToLower()) != null)
        {
            _logger.LogWarning("{timestamp}: Registration failed for Email: {Email} from IP: {IpAddress} - Email exists.", userRegisterDto.Email, ipAddress, timestamp);
            return new BadRequestObjectResult("Email already exists.");
        }
        IdentityResult result = await userManager.CreateAsync(user, userRegisterDto.Password);

        if (!result.Succeeded)
            return new BadRequestObjectResult(new { Message = "Registration failed.", result.Errors });
        _logger.LogInformation("{timestamp}: Registration successful for Email: {Email} from IP: {IpAddress}.", userRegisterDto.Email, ipAddress, timestamp);
        return new OkObjectResult("Registration successful.");
    }


    public async Task<ActionResult<List<ArduinoInfoDTO>>> GetUnusedArduino(Elder elder)
    {
        Location elderLocation = new Location
        {
            Latitude = elder.Latitude,
            Longitude = elder.Longitude
        };
        List<Elder> elders = await _elderRepository.Query().ToListAsync();
        List<GPSData> gpsData = await _gpsRepository.Query().ToListAsync();
        List<GPSData> filteredGpsData = gpsData.Where(g => elders.All(e => e.MacAddress != g.MacAddress)).ToList();
        List<ArduinoInfoDTO> addressNotAssociated = [];
        foreach (GPSData gps in filteredGpsData)
        {
            string GpsAddress = await _geoService.GetAddressFromCoordinates(gps.Latitude, gps.Longitude);
            float distance = GeoService.CalculateDistance(new Location { Latitude = gps.Latitude, Longitude = gps.Longitude }, elderLocation);
            int minutesSinceActivity = ((int)(DateTime.UtcNow - gps.Timestamp).TotalMinutes) * -1;
            if (!(distance < 0.5)) continue;
            _logger.LogInformation("Distance: {Distance} km, Address: {GpsAddress}, Minutes since activity: {minutesSinceActivity}", distance, GpsAddress, minutesSinceActivity);
            ArduinoInfoDTO arduinoInfo = new ArduinoInfoDTO
            {
                Id = gps.Id,
                MacAddress = gps.MacAddress,
                Address = GpsAddress,
                Distance = distance,
                LastActivity = minutesSinceActivity
            };
            addressNotAssociated.Add(arduinoInfo);
        }
        _logger.LogInformation("Address not associated count: {addressNotAssociated}", addressNotAssociated.Count);
        return addressNotAssociated.Count != 0 ? addressNotAssociated : [];
    }
}