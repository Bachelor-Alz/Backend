using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthDevice.Data;

public class DbHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _dbContext;

    public DbHealthCheck(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            bool canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("Database connection OK")
                : HealthCheckResult.Unhealthy("Database connection failed");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database exception: " + ex.Message);
        }
    }
}
