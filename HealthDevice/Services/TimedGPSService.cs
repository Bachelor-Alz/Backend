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
                    ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    GeoService geoService = scope.ServiceProvider.GetRequiredService<GeoService>();
                    List<Elder> elders = elderManager.Users.ToList();
                    //Find all gpsData where the address isnt assigned to an elder arduino
                    List<GPS> gpsData = dbContext.GPSData.Where(g => g.Address != null && elders.All(e => e.Arduino != g.Address)).ToList();
                    
                    foreach (Elder elder in elders)
                    {
                        if (elder.Arduino == null)
                        {
                            foreach (GPS gp in gpsData)
                            {
                                string GpsAddress = await geoService.GetAddressFromCoordinates(gp.Latitude, gp.Longitude);
                                if (elder is not { latitude: not null, longitude: not null }) continue;
                                string elderAddress = await geoService.GetAddressFromCoordinates((double)elder.latitude, (double)elder.longitude);
                                if (GpsAddress != elderAddress) continue;
                                elder.Arduino = gp.Address;
                                _logger.LogInformation("Elder {ElderEmail} assigned to Arduino {Arduino}", elder.Email, gp.Address);
                                
                            }
                        }
                        DateTime currentTime = DateTime.Now;
                        
                        Location location = await healthService.GetLocation(currentTime, elder);
                        elder.Location = location;
                        
                        await elderManager.UpdateAsync(elder);
                        await healthService.ComputeOutOfPerimeter(elder);
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}