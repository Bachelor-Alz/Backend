using HealthDevice.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// ReSharper disable SuggestVarOrType_SimpleTypes

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
    private readonly IRepository<GPSData> _gpsDataRepository;

    public AiService(ILogger<AiService> logger, IEmailService emailService, IGeoService geoService,
        IRepository<Elder> elderRepository, IRepository<Caregiver> caregiverRepository,
        IRepository<FallInfo> fallInfoRepository, IRepository<Location> locationRepository, IRepository<GPSData> gpsDataRepository)
    {
        _logger = logger;
        _emailService = emailService;
        _geoService = geoService;
        _elderRepository = elderRepository;
        _caregiverRepository = caregiverRepository;
        _fallInfoRepository = fallInfoRepository;
        _locationRepository = locationRepository;
        _gpsDataRepository = gpsDataRepository;
    }

    public async Task HandleAiRequest([FromBody] List<int> request, string address)
    {
        int count = 0;
        foreach (int t in request)
        {
            if (t == 0)
                count = 0;
            else
            {
                count++;
                if (count < 4) continue;
                _logger.LogInformation("Fall detected for elder {address}", address);
                await HandleFall(address);
                return;
            }
        }

        _logger.LogInformation("No fall detected for elder {address}", address);
    }

    private async Task HandleFall(string macAddress)
    {
        FallInfo fallInfo = new FallInfo()
        {
            Timestamp = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
                DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0).ToUniversalTime(),
            Location = new Location(),
            MacAddress = macAddress
        };
        await _fallInfoRepository.Add(fallInfo);

        try
        {
            _logger.LogInformation("Handling fall for elder with MacAddress: {MacAddress}", macAddress);
            Elder? elder = await _elderRepository.Query().FirstOrDefaultAsync(m => m.MacAddress == macAddress);
            if (elder == null)
            {
                return;
            }
             

            List<Caregiver> caregivers = await _caregiverRepository.Query()
                .Where(e => e.Elders != null && e.Elders.Contains(elder)).ToListAsync();
            GPSData? location = await _gpsDataRepository.Query().Where(a => a.MacAddress == elder.MacAddress)
                .OrderByDescending(a => a.Timestamp).FirstOrDefaultAsync();
            _logger.LogInformation("{location}", location);
            if (location == null || caregivers.Count == 0)
            {
                return;
            }
               

            string address = await _geoService.GetAddressFromCoordinates(location.Latitude, location.Longitude);
            await _emailService.SendEmail("Fall detected",
                $"Fall detected for elder {elder.Name} at location {address}.", elder);
            _logger.LogInformation("Fall email sent to elder: {ElderEmail}", elder.Email);
        }
        catch
        {
            _logger.LogError("Failed to handle fall");
        }
    }
}