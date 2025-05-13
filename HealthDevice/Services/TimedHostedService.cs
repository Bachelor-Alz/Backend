using HealthDevice.Models;
using Microsoft.EntityFrameworkCore;

// ReSharper disable SuggestVarOrType_SimpleTypes

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
                    // Resolving scoped services within the scope
                    IRepository<Elder> elderRepository = scope.ServiceProvider.GetRequiredService<IRepository<Elder>>();
                    IRepository<Heartrate> hrRepository =
                        scope.ServiceProvider.GetRequiredService<IRepository<Heartrate>>();
                    IRepository<Spo2> spo2Repository = scope.ServiceProvider.GetRequiredService<IRepository<Spo2>>();
                    IRepository<DistanceInfo> distanceRepository =
                        scope.ServiceProvider.GetRequiredService<IRepository<DistanceInfo>>();
                    var healthService = scope.ServiceProvider.GetRequiredService<IHealthService>();

                    List<Elder> elders = await elderRepository.Query().ToListAsync(cancellationToken: stoppingToken);

                    foreach (Elder elder in elders)
                    {
                        string? macAddress = elder.MacAddress;
                        if (macAddress == null) continue;
                        DateTime oldTime = DateTime.UtcNow.AddMonths(-3);

                        await healthService.DeleteData<Heartrate>(oldTime, macAddress);
                        await healthService.DeleteData<Spo2>(oldTime, macAddress);
                        await healthService.DeleteData<DistanceInfo>(oldTime, macAddress);
                        await healthService.DeleteData<Steps>(oldTime, macAddress);
                        await healthService.DeleteData<FallInfo>(oldTime, macAddress);

                        await elderRepository.Update(elder);
                    }
                }

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}