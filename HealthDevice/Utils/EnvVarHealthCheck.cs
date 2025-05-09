using Microsoft.Extensions.Diagnostics.HealthChecks;

public class EnvVarHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        string[] requiredVars = new[] { "ConnectionStrings__DefaultConnection", "IS_TESTING" };
        string[] optionalVars = new[] { "SMTP_HOST", "SMTP_PORT", "SMTP_USER", "SMTP_PASSWORD" };

        List<string> missingRequired = requiredVars
            .Where(v => string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(v)))
            .ToList();

        List<string> missingOptional = optionalVars
            .Where(v => string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(v)))
            .ToList();

        List<string> allMissing = missingRequired.Concat(missingOptional).ToList();
        string message = allMissing.Any()
            ? "Missing environment variables: " + string.Join(", ", allMissing)
            : "All environment variables are set";

        if (missingRequired.Any())
            return Task.FromResult(HealthCheckResult.Unhealthy(message));
        if (missingOptional.Any())
            return Task.FromResult(HealthCheckResult.Degraded(message));

        return Task.FromResult(HealthCheckResult.Healthy(message));
    }
}
