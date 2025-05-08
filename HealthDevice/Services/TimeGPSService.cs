using HealthDevice.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthDevice.Services
{
    public class TimedGPSService : BackgroundService
    {
        private readonly ILogger<TimedGPSService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRepository<Elder> _elderRepository;

        public TimedGPSService(ILogger<TimedGPSService> logger, IServiceProvider serviceProvider, IRepository<Elder> elderRepository)    
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _elderRepository = elderRepository;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Timed GPS Service is working.");

                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    var repositoryFactory = scope.ServiceProvider.GetRequiredService<IRepositoryFactory>();
                    var healthService = scope.ServiceProvider.GetRequiredService<IHealthService>();
                    List<Elder> elders = await _elderRepository.Query().Where(e => e.MacAddress != null).ToListAsync(cancellationToken: stoppingToken);
                    
                    foreach (string arduino in elders.Select(elder => elder.MacAddress).OfType<string>())
                    {
                        Location location = await healthService.GetLocation(DateTime.UtcNow, arduino);
                        if (location.Latitude != 0 && location.Longitude != 0)
                        {
                            await healthService.ComputeOutOfPerimeter(arduino, location);
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}