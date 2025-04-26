using HealthDevice.Data;
using HealthDevice.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
                    var repositoryFactory = scope.ServiceProvider.GetRequiredService<IRepositoryFactory>();
                    var elderRepository = repositoryFactory.GetRepository<Elder>();
                    var healthService = scope.ServiceProvider.GetRequiredService<HealthService>();
                    List<Elder> elders = await elderRepository.Query().Where(e => e.MacAddress != null).ToListAsync(cancellationToken: stoppingToken);
                    
                    foreach (string arduino in elders.Select(elder => elder.MacAddress).OfType<string>())
                    {
                        Location location = await healthService.GetLocation(DateTime.UtcNow, arduino);
                        await healthService.ComputeOutOfPerimeter(arduino, location);
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}