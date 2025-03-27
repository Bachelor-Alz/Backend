// TimedHostedService.cs
using HealthDevice.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
                    var elders = elderManager.Users.ToList();

                    foreach (var elder in elders)
                    {
                        DateTime currentTime = DateTime.Now;

                        // Update heart rate
                        var heartRate = await healthService.CalculateHeartRate(currentTime, elder);
                        elder.heartRates.Add(heartRate);

                        // Update SpO2
                        var spo2 = await healthService.CalculateSpo2(currentTime, elder);
                        elder.spo2s.Add(spo2);

                        // Update the elder in the database
                        await elderManager.UpdateAsync(elder);
                    }
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}