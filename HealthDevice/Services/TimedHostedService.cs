using HealthDevice.Data;
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

                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    UserManager<Elder> elderManager = scope.ServiceProvider.GetRequiredService<UserManager<Elder>>();
                    HealthService healthService = scope.ServiceProvider.GetRequiredService<HealthService>();
                    ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    List<Elder> elders = elderManager.Users.ToList();

                    foreach (Elder elder in elders)
                    {
                        string? arduino = elder.MacAddress;
                        if (arduino == null) continue;
                        DateTime currentTime = DateTime.UtcNow;
                        
                        List<Heartrate> heartRate = await healthService.CalculateHeartRate(currentTime, arduino);
                        db.Heartrate.AddRange(heartRate);

                        List<Spo2> spo2 = await healthService.CalculateSpo2(currentTime, arduino);
                        db.SpO2.AddRange(spo2);

                        Kilometer distance = await healthService.CalculateDistanceWalked(currentTime, arduino);
                        db.Distance.Add(distance);

                        await healthService.DeleteMax30102Data(currentTime, arduino);
                        await healthService.DeleteGpsData(currentTime, arduino);
                        
                        await elderManager.UpdateAsync(elder);

                    }
                }

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}