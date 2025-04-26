using HealthDevice.DTO;
using Microsoft.AspNetCore.Mvc;

namespace HealthDevice.Services;

public class AiService : IAIService
{
    
    private readonly ILogger<AiService> _logger;
    private readonly EmailService _emailService;
    private readonly GeoService _geoService;
    private readonly IRepositoryFactory _repositoryFactory;
    
    public AiService(ILogger<AiService> logger, EmailService emailService, GeoService geoService, IRepositoryFactory repositoryFactory)
    {
        _logger = logger;
        _emailService = emailService;
        _geoService = geoService;
        _repositoryFactory = repositoryFactory;
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
        IRepository<Elder> elderRepository = _repositoryFactory.GetRepository<Elder>();
        IRepository<Caregiver> caregiverRepository = _repositoryFactory.GetRepository<Caregiver>();
        IRepository<FallInfo> fallInfoRepository = _repositoryFactory.GetRepository<FallInfo>();
        IRepository<Location> locationRepository = _repositoryFactory.GetRepository<Location>();
        FallInfo fallInfo = new FallInfo()
        {
            Timestamp = DateTime.UtcNow,
            Location = new Location(),
            MacAddress = addrees
        };
        fallInfoRepository.Add(fallInfo);
        try
        {
            Elder? elder = elderRepository.Query().FirstOrDefault(m => m.MacAddress == addrees);
            if (elder == null)
            {
                _logger.LogWarning("Elder {address} does not exist", addrees);
                return;
            }
            List<Caregiver> caregivers = caregiverRepository.Query().Where(e => e.Elders != null && e.Elders.Contains(elder)).ToList();
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
                Location? location = locationRepository.Query().Where(a => a.MacAddress == elder.MacAddress).OrderByDescending(a => a.Timestamp).FirstOrDefault();
                if (location == null) continue;
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
        catch
        {
           _logger.LogError("Failed to handle fall");
        }
    }
    }