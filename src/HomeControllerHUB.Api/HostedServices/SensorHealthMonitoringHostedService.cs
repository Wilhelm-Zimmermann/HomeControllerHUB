using System.Diagnostics;
using HomeControllerHUB.Application.Sensors.Commands.MonitorSensorHealth;
using MediatR;
using Microsoft.Extensions.Options;

namespace HomeControllerHUB.Api.HostedServices;

public class SensorHealthMonitoringHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<SensorHealthMonitoringHostedService> _logger;
    private readonly SensorHealthMonitoringOptions _options;

    public SensorHealthMonitoringHostedService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<SensorHealthMonitoringHostedService> logger,
        IOptions<SensorHealthMonitoringOptions> options)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Sensor health monitoring is disabled");
            return;
        }

        var interval = TimeSpan.FromSeconds(_options.EffectiveIntervalSeconds);

        await RunMonitoringAsync(stoppingToken);

        using var timer = new PeriodicTimer(interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunMonitoringAsync(stoppingToken);
        }
    }

    private async Task RunMonitoringAsync(CancellationToken stoppingToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var result = await mediator.Send(new MonitorSensorHealthCommand(), stoppingToken);

            stopwatch.Stop();
            _logger.LogInformation(
                "Sensor health monitoring completed. SensorsChecked={SensorsChecked} OfflineAlertsCreated={OfflineAlertsCreated} LowBatteryAlertsCreated={LowBatteryAlertsCreated} DuplicateAlertsSkipped={DuplicateAlertsSkipped} DurationMs={DurationMs}",
                result.SensorsChecked,
                result.OfflineAlertsCreated,
                result.LowBatteryAlertsCreated,
                result.DuplicateAlertsSkipped,
                stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Error occurred during sensor health monitoring. DurationMs={DurationMs}",
                stopwatch.ElapsedMilliseconds);
        }
    }
}
