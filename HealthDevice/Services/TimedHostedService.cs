using HealthDevice.DTO;
using Microsoft.AspNetCore.Identity;

namespace HealthDevice.Services
{
    public class TimedHostedService : BackgroundService
    {
        private readonly ILogger<TimedHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public TimedHostedService(ILogger<TimedHostedService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Timed Hosted Service is working.");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var elderManager = scope.ServiceProvider.GetRequiredService<UserManager<Elder>>();
                    var healthService = scope.ServiceProvider.GetRequiredService<HealthService>();
                    List<Elder> elders = elderManager.Users.ToList();

                    foreach (Elder elder in elders)
                    {
                        DateTime currentTime = DateTime.Now;
                        
                        Heartrate heartRate = await healthService.CalculateHeartRate(currentTime, elder);
                        elder.heartRates.Add(heartRate);
                        
                        Spo2 spo2 = await healthService.CalculateSpo2(currentTime, elder);
                        elder.spo2s.Add(spo2);
                        
                        await elderManager.UpdateAsync(elder);
                    }
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}