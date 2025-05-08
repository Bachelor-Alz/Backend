using HealthDevice.DTO;
using HealthDevice.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthDevice.Services;

public class AiService : IAIService
{

    private readonly ILogger<AiService> _logger;
    private readonly IEmailService _emailService;
    private readonly IGeoService _geoService;
    private readonly IRepository<Elder> _elderRepository;
    private readonly IRepository<Caregiver> _caregiverRepository;
    private readonly IRepository<FallInfo> _fallInfoRepository;
    private readonly IRepository<Location> _locationRepository;

    public AiService(ILogger<AiService> logger, IEmailService emailService, IGeoService geoService, IRepository<Elder> elderRepository, IRepository<Caregiver> caregiverRepository, IRepository<FallInfo> fallInfoRepository, IRepository<Location> locationRepository)
    {
        _logger = logger;
        _emailService = emailService;
        _geoService = geoService;
        _elderRepository = elderRepository;
        _caregiverRepository = caregiverRepository;
        _fallInfoRepository = fallInfoRepository;
        _locationRepository = locationRepository;
    }

    public async Task HandleAiRequest([FromBody] List<int> request, string address)
    {
        string count = "";
        foreach (int t in request)
        {
            count += t.ToString();
            if (count.Contains("0"))
            {
                count = "";
            }
            if (count.Length >= 4)
            {
                _logger.LogInformation("Fall detected for elder {address}", address);
                await HandleFall(address);
                return;
            }
        }
        _logger.LogInformation("No fall detected for elder {address}", address);
    }

    private async Task HandleFall(string addrees)
    {
        FallInfo fallInfo = new FallInfo()
        {
            Timestamp = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0),
            Location = new Location(),
            MacAddress = addrees
        };
        await _fallInfoRepository.Add(fallInfo);
        try
        {
            Elder? elder = await _elderRepository.Query().FirstOrDefaultAsync(m => m.MacAddress == addrees);
            if (elder == null)
            {
                _logger.LogWarning("Elder {address} does not exist", addrees);
                return;
            }
            List<Caregiver> caregivers = await _caregiverRepository.Query().Where(e => e.Elders != null && e.Elders.Contains(elder)).ToListAsync();
            if (caregivers.Count == 0)
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
                Location? location = await _locationRepository.Query().Where(a => a.MacAddress == elder.MacAddress).OrderByDescending(a => a.Timestamp).FirstOrDefaultAsync();
                if (location == null) continue;
                string address = await _geoService.GetAddressFromCoordinates(location.Latitude, location.Longitude);

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
        catch
        {
            _logger.LogError("Failed to handle fall");
        }
    }
}