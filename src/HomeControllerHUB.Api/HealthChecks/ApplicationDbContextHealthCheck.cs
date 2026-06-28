using HomeControllerHUB.Infra.DatabaseContext;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HomeControllerHUB.Api.HealthChecks;

public class ApplicationDbContextHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;

    public ApplicationDbContextHealthCheck(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);

            return canConnect
                ? HealthCheckResult.Healthy("Database connection is available")
                : HealthCheckResult.Unhealthy("Database connection is not available");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}
