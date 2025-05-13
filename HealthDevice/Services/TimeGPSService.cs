using HealthDevice.Models;
using Microsoft.EntityFrameworkCore;
// ReSharper disable SuggestVarOrType_SimpleTypes

namespace HealthDevice.Services
{
    public class TimedGPSService : BackgroundService
    {
        private readonly ILogger<TimedGPSService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRepository<Elder> _elderRepository;
        private readonly IRepository<DistanceInfo> distanceRepository;

        public TimedGPSService(ILogger<TimedGPSService> logger, IServiceProvider serviceProvider, IRepository<Elder> elderRepository, IRepository<DistanceInfo> distanceRepository)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _elderRepository = elderRepository;
            this.distanceRepository = distanceRepository;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Timed GPS Service is working.");
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    var healthService = scope.ServiceProvider.GetRequiredService<IHealthService>();
                    List<Elder> elders = await _elderRepository.Query().Where(e => e.MacAddress != null).ToListAsync(cancellationToken: stoppingToken);

                    foreach (string arduino in elders.Select(elder => elder.MacAddress).OfType<string>())
                    {
                        DistanceInfo distance = await healthService.CalculateDistanceWalked(DateTime.UtcNow, arduino);
                        await distanceRepository.Add(distance);
                        
                        Location location = await healthService.GetLocation(DateTime.UtcNow, arduino);
                        if (location.Latitude != 0 && location.Longitude != 0)
                        {
                            await healthService.ComputeOutOfPerimeter(arduino, location);
                        }
                        await healthService.DeleteGpsData(DateTime.UtcNow, arduino);
                    }
                    
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}