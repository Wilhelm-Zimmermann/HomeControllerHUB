using System.Net.Sockets;
using HomeControllerHUB.Api.Settings;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace HomeControllerHUB.Api.HealthChecks;

public class RabbitMqHealthCheck : IHealthCheck
{
    private readonly IHostEnvironment _environment;
    private readonly RabbitMqSettings _settings;

    public RabbitMqHealthCheck(
        IHostEnvironment environment,
        IOptions<RabbitMqSettings> settings)
    {
        _environment = environment;
        _settings = settings.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (_environment.IsEnvironment("Testing"))
        {
            return HealthCheckResult.Healthy("RabbitMQ is replaced by the in-memory bus during tests");
        }

        try
        {
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(_settings.Host, _settings.Port, cancellationToken);

            return HealthCheckResult.Healthy("RabbitMQ endpoint is reachable");
        }
        catch (Exception ex) when (ex is SocketException or OperationCanceledException or IOException)
        {
            return HealthCheckResult.Unhealthy("RabbitMQ endpoint is not reachable", ex);
        }
    }
}
