using HealthDevice.Data;
using HealthDevice.DTO;
using Microsoft.AspNetCore.Identity;

namespace HealthDevice.Services
{
    public class TimedGPSService : BackgroundService
    {
        private readonly ILogger<TimedGPSService> _logger;
        private readonly IServiceProvider _serviceProvider;
        
        public TimedGPSService(ILogger<TimedGPSService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Timed GPS Service is working.");

                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    UserManager<Elder> elderManager = scope.ServiceProvider.GetRequiredService<UserManager<Elder>>();
                    HealthService healthService = scope.ServiceProvider.GetRequiredService<HealthService>();
                    ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    GeoService geoService = scope.ServiceProvider.GetRequiredService<GeoService>();
                    List<Elder> elders = elderManager.Users.ToList();
                    List<GPS> gpsData = db.GPSData.ToList();
                    var filteredGpsData = gpsData.Where(g => elders.All(e => e.Arduino != g.Address)).ToList();
                    
                    foreach (Elder elder in elders)
                    {
                        string? arduino = elder.Arduino;
                        if (arduino == null) continue;
                        DateTime currentTime = DateTime.UtcNow;
                        
                        Location location = await healthService.GetLocation(currentTime, arduino);
                        db.Location.Add(location);
                        foreach (GPS gp in filteredGpsData)

                        {
                            string GpsAddress = await geoService.GetAddressFromCoordinates(gp.Latitude, gp.Longitude);
                            if (elder is not { latitude: not null, longitude: not null }) continue;
                            string elderAddress = await geoService.GetAddressFromCoordinates((double)elder.latitude, (double)elder.longitude);
                            if (GpsAddress != elderAddress) continue;
                            elder.Arduino = gp.Address;
                            _logger.LogInformation("Elder {ElderEmail} assigned to Arduino {Arduino}", elder.Email, gp.Address);
                        }

                        await db.SaveChangesAsync();
                        await elderManager.UpdateAsync(elder);
                        await healthService.ComputeOutOfPerimeter(arduino, location);
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(100), stoppingToken);
            }
        }
    }
}