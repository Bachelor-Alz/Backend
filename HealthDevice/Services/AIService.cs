using HealthDevice.Data;
using HealthDevice.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HealthDevice.Services;

public class AiService
{
    
    private readonly ILogger<AiService> _logger;
    private readonly UserManager<Elder> _elderManager;
    private readonly UserManager<Caregiver> _caregiverManager;
    private readonly EmailService _emailService;
    private readonly GeoService _geoService;
    private readonly ApplicationDbContext _db;
    
    public AiService(ILogger<AiService> logger, UserManager<Elder> elderManager, UserManager<Caregiver> caregiverManager, EmailService emailService, GeoService geoService, ApplicationDbContext db)
    {
        _logger = logger;
        _elderManager = elderManager;
        _caregiverManager = caregiverManager;
        _emailService = emailService;
        _geoService = geoService;
        _db = db;
    }
    public async Task HandleAiRequest([FromBody] List<int> request, string address)
    {
       if (request.Contains(1))
       {
              _logger.LogInformation("Fall detected for elder {address}", address);
           await HandleFall(address);
       }
    }

    private async Task HandleFall(string addrees)
    {
        FallInfo fallInfo = new FallInfo()
        {
            Timestamp = DateTime.UtcNow,
            Location = new Location(),
            MacAddress = addrees
        };
        _db.FallInfo.Add(fallInfo);
        try
        {
            await _db.SaveChangesAsync();
            Elder elder = _elderManager.Users.FirstOrDefault(elder => elder.Arduino == addrees);
            if (elder == null)
            {
                _logger.LogWarning("Elder {address} does not exist", addrees);
                return;
            }
            List<Caregiver> caregivers = _caregiverManager.Users.Where(e => e.Elders != null && e.Elders.Contains(elder)).ToList();
            if(caregivers.Count == 0)
            {
                _logger.LogWarning("No caregivers found for elder {elder}", elder.Email);
                return;
            }
            foreach (var caregiver in caregivers)
            {
                Email emailInfo = new Email
                {
                    name = caregiver.Name,
                    email = caregiver.Email
                };
                if (emailInfo.name == null || emailInfo.email == null)
                {
                    _logger.LogWarning("No email found for caregiver {caregiver}", caregiver.Email);
                    return;
                }
                Location? location = _db.Location.Where(a => a.MacAddress == elder.Arduino).OrderByDescending(a => a.Timestamp).FirstOrDefault();
                if (location != null)
                {
                    string address = await _geoService.GetAddressFromCoordinates(location.Latitude,location.Longitude);

                    _logger.LogInformation("Sending email to {caregiver}", caregiver.Email);
                    try
                    {
                        await _emailService.SendEmail(emailInfo, "Fall detected",
                            $"Fall detected for elder {elder.Name} at location {address}.");
                    }
                    catch
                    {
                        _logger.LogError("Failed to send email to {caregiver}", caregiver.Email);
                        return;
                    }
                }
            }
        }
        catch
        {
           _logger.LogError("Failed to handle fall");
        }
    }
    }