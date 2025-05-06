using HealthDevice.DTO;
using Microsoft.EntityFrameworkCore;

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
                    var repositoryFactory = scope.ServiceProvider.GetRequiredService<IRepositoryFactory>();
                    var elderRepository = repositoryFactory.GetRepository<Elder>();
                    var healthService = scope.ServiceProvider.GetRequiredService<IHealthService>();
                    var hrRepository = repositoryFactory.GetRepository<Heartrate>();
                    var spo2Repository = repositoryFactory.GetRepository<Spo2>();
                    var distanceRepository = repositoryFactory.GetRepository<Kilometer>();
                    List<Elder> elders = await elderRepository.Query().ToListAsync(cancellationToken: stoppingToken);

                    foreach (Elder elder in elders)
                    {
                        string? arduino = elder.MacAddress;
                        if (arduino == null) continue;
                        DateTime lastMonth = DateTime.UtcNow.AddDays(-30);
                        
                        List<Heartrate> heartRate = await healthService.CalculateHeartRate(lastMonth, arduino);
                        await hrRepository.AddRange(heartRate);

                        List<Spo2> spo2 = await healthService.CalculateSpo2(lastMonth, arduino);
                        await spo2Repository.AddRange(spo2);

                        Kilometer distance = await healthService.CalculateDistanceWalked(lastMonth, arduino);
                        await distanceRepository.Add(distance);

                        await healthService.DeleteMax30102Data(lastMonth, arduino);
                        await healthService.DeleteGpsData(lastMonth, arduino);

                        await elderRepository.Update(elder);

                    }
                }

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}