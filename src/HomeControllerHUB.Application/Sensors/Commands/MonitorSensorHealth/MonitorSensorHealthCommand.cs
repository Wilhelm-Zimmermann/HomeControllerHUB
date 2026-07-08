using System.Globalization;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HomeControllerHUB.Application.Sensors.Commands.MonitorSensorHealth;

public class MonitorSensorHealthCommand : IRequest<MonitorSensorHealthResult>
{
}

public class MonitorSensorHealthResult
{
    public int SensorsChecked { get; set; }
    public int OfflineAlertsCreated { get; set; }
    public int LowBatteryAlertsCreated { get; set; }
    public int DuplicateAlertsSkipped { get; set; }
}

public class MonitorSensorHealthCommandHandler : IRequestHandler<MonitorSensorHealthCommand, MonitorSensorHealthResult>
{
    private readonly ApplicationDbContext _context;
    private readonly IDateTime _dateTime;
    private readonly ISharedResource _sharedResource;
    private readonly SensorHealthMonitoringOptions _options;

    public MonitorSensorHealthCommandHandler(
        ApplicationDbContext context,
        IDateTime dateTime,
        ISharedResource sharedResource,
        IOptions<SensorHealthMonitoringOptions> options)
    {
        _context = context;
        _dateTime = dateTime;
        _sharedResource = sharedResource;
        _options = options.Value;
    }

    public async Task<MonitorSensorHealthResult> Handle(
        MonitorSensorHealthCommand request,
        CancellationToken cancellationToken)
    {
        var now = _dateTime.UtcNow;
        var offlineThresholdMinutes = _options.EffectiveOfflineThresholdMinutes;
        var offlineCutoff = now.AddMinutes(-offlineThresholdMinutes);

        var activeSensors = await _context.Sensors
            .Where(sensor => sensor.IsActive)
            .ToListAsync(cancellationToken);

        var result = new MonitorSensorHealthResult
        {
            SensorsChecked = activeSensors.Count
        };

        if (activeSensors.Count == 0)
        {
            return result;
        }

        var sensorIds = activeSensors.Select(sensor => sensor.Id).ToList();
        var pendingAlerts = await _context.SensorAlerts
            .AsNoTracking()
            .Where(alert => sensorIds.Contains(alert.SensorId)
                            && !alert.IsAcknowledged
                            && (alert.Type == AlertType.DeviceOffline || alert.Type == AlertType.BatteryLow))
            .Select(alert => new { alert.SensorId, alert.Type })
            .ToListAsync(cancellationToken);

        var pendingAlertKeys = pendingAlerts
            .Select(alert => (alert.SensorId, alert.Type))
            .ToHashSet();

        var alertsToCreate = new List<SensorAlert>();

        foreach (var sensor in activeSensors)
        {
            if (IsOffline(sensor, offlineCutoff))
            {
                TryAddAlert(
                    sensor,
                    AlertType.DeviceOffline,
                    CreateOfflineMessage(sensor, offlineThresholdMinutes),
                    now,
                    pendingAlertKeys,
                    alertsToCreate,
                    result,
                    created => result.OfflineAlertsCreated = created);
            }

            if (IsLowBattery(sensor, _options.LowBatteryThreshold))
            {
                TryAddAlert(
                    sensor,
                    AlertType.BatteryLow,
                    CreateLowBatteryMessage(sensor),
                    now,
                    pendingAlertKeys,
                    alertsToCreate,
                    result,
                    created => result.LowBatteryAlertsCreated = created);
            }
        }

        if (alertsToCreate.Count > 0)
        {
            await _context.SensorAlerts.AddRangeAsync(alertsToCreate, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    private static bool IsOffline(Sensor sensor, DateTime offlineCutoff)
    {
        return sensor.LastCommunication == default || sensor.LastCommunication < offlineCutoff;
    }

    private static bool IsLowBattery(Sensor sensor, double lowBatteryThreshold)
    {
        return sensor.BatteryLevel.HasValue && sensor.BatteryLevel.Value < lowBatteryThreshold;
    }

    private static void TryAddAlert(
        Sensor sensor,
        AlertType alertType,
        string message,
        DateTime timestamp,
        ISet<(Guid SensorId, AlertType AlertType)> pendingAlertKeys,
        ICollection<SensorAlert> alertsToCreate,
        MonitorSensorHealthResult result,
        Action<int> updateCreatedCount)
    {
        var alertKey = (sensor.Id, alertType);
        if (pendingAlertKeys.Contains(alertKey))
        {
            result.DuplicateAlertsSkipped++;
            return;
        }

        alertsToCreate.Add(new SensorAlert
        {
            SensorId = sensor.Id,
            Type = alertType,
            Message = message,
            Timestamp = timestamp,
            IsAcknowledged = false
        });

        pendingAlertKeys.Add(alertKey);
        updateCreatedCount(alertType == AlertType.DeviceOffline
            ? result.OfflineAlertsCreated + 1
            : result.LowBatteryAlertsCreated + 1);
    }

    private string CreateOfflineMessage(Sensor sensor, int offlineThresholdMinutes)
    {
        return string.Format(
            CultureInfo.CurrentCulture,
            _sharedResource.Message("SensorOffline"),
            sensor.Name,
            offlineThresholdMinutes);
    }

    private string CreateLowBatteryMessage(Sensor sensor)
    {
        return $"{_sharedResource.Message("SensorLowBattery")}: {sensor.Name}";
    }
}
