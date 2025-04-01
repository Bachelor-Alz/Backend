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
                    List<Elder> elders = elderManager.Users.ToList();

                    foreach (Elder elder in elders)
                    {
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