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
    
    public AiService(ILogger<AiService> logger, UserManager<Elder> elderManager, UserManager<Caregiver> caregiverManager, EmailService emailService, GeoService geoService)
    {
        _logger = logger;
        _elderManager = elderManager;
        _caregiverManager = caregiverManager;
        _emailService = emailService;
        _geoService = geoService;
    }
    public async Task HandleAiRequest([FromBody] List<int> request, string address)
    {
        Elder? elder = _elderManager.Users.FirstOrDefault(a => a.Arduino == address);
        if (elder == null)
        {
            _logger.LogWarning("No elder found with address {address}", address);
            return;
        }
        _logger.LogInformation("HandleAIRequest {request}", request);
       if (request.Contains(1))
       {
           await HandleFall(elder);
       }
    }

    private async Task HandleFall(Elder elder)
    {
           
        FallInfo fallInfo = new FallInfo()
        {
            Timestamp = DateTime.Now,
            Location = new Location(),
        };
        elder.FallInfo ??= new List<FallInfo>();
        elder.FallInfo.Add(fallInfo);
        try
        {
            await _elderManager.UpdateAsync(elder);
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

                if (elder.Location != null)
                {
                    string address = await _geoService.GetAddressFromCoordinates(elder.Location.Latitude, elder.Location.Longitude);

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
           _logger.LogError("Failed to update elder {elder}", elder.Email);
        }
    }
    }