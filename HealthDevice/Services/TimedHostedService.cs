using HealthDevice.DTO;
using Microsoft.AspNetCore.Identity;

namespace HealthDevice.Services
{
    public class TimedHostedService(ILogger<TimedHostedService> logger, IServiceProvider serviceProvider)
        : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("Timed Hosted Service is working.");

                using (IServiceScope scope = serviceProvider.CreateScope())
                {
                    UserManager<Elder> elderManager = scope.ServiceProvider.GetRequiredService<UserManager<Elder>>();
                    HealthService healthService = scope.ServiceProvider.GetRequiredService<HealthService>();
                    List<Elder> elders = elderManager.Users.ToList();

                    foreach (Elder elder in elders)
                    {
                        DateTime currentTime = DateTime.Now;
                        
                        Heartrate heartRate = await healthService.CalculateHeartRate(currentTime, elder);
                        elder.Heartrate.Add(heartRate);
                        
                        Spo2 spo2 = await healthService.CalculateSpo2(currentTime, elder);
                        elder.SpO2.Add(spo2);

                        Kilometer distance = await healthService.CalculateDistanceWalked(currentTime, elder);
                        elder.Distance.Add(distance);
                        
                        await HealthService.DeleteMax30102Data(currentTime, elder);
                        await HealthService.DeleteGpsData(currentTime, elder);
                        
                        await elderManager.UpdateAsync(elder);
                    }
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}