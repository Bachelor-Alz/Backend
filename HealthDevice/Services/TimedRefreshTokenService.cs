

using HealthDevice.Data;
using Microsoft.EntityFrameworkCore;

public class TimedRefreshTokenService : BackgroundService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<TimedRefreshTokenService> _logger;

    public TimedRefreshTokenService(ApplicationDbContext dbContext, ILogger<TimedRefreshTokenService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Refresh Token Service is starting.");
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);

            var expiredTokens = await _dbContext.RefreshToken
                .Where(rt => rt.IsExpired)
                .ToListAsync(stoppingToken);

            _dbContext.RefreshToken.RemoveRange(expiredTokens);
            await _dbContext.SaveChangesAsync(stoppingToken);
        }
        _logger.LogInformation("Timed Refresh Token Service is done.");
        await Task.Delay(TimeSpan.FromDays(1), stoppingToken);

    }
}