using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Dashboard.Queries.GetDashboardSummary;

[Authorize(Domain = DomainNames.IoT, Action = SecurityActionType.Read)]
public class GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>
{
    public Guid? EstablishmentId { get; set; }
}

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private const int LowBatteryThreshold = 15;
    private const int RecentAlertsLimit = 5;

    private readonly ApplicationDbContext _context;

    public GetDashboardSummaryQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var last24Hours = DateTime.UtcNow.AddHours(-24);
        var sensorsQuery = _context.Sensors.AsNoTracking().AsQueryable();
        var locationsQuery = _context.Locations.AsNoTracking().AsQueryable();
        var alertsQuery = _context.SensorAlerts.AsNoTracking().AsQueryable();
        var readingsQuery = _context.SensorReadings.AsNoTracking().AsQueryable();

        if (request.EstablishmentId.HasValue)
        {
            var establishmentId = request.EstablishmentId.Value;

            sensorsQuery = sensorsQuery.Where(sensor => sensor.EstablishmentId == establishmentId);
            locationsQuery = locationsQuery.Where(location => location.EstablishmentId == establishmentId);
            alertsQuery = alertsQuery.Where(alert => alert.Sensor.EstablishmentId == establishmentId);
            readingsQuery = readingsQuery.Where(reading => reading.Sensor.EstablishmentId == establishmentId);
        }

        var recentAlerts = await alertsQuery
            .OrderByDescending(alert => alert.Created)
            .Take(RecentAlertsLimit)
            .Select(alert => new DashboardRecentAlertDto
            {
                Id = alert.Id,
                Type = alert.Type,
                Message = alert.Message,
                SensorId = alert.SensorId,
                SensorName = alert.Sensor.Name,
                SensorDeviceId = alert.Sensor.DeviceId,
                LocationName = alert.Sensor.Location.Name,
                IsAcknowledged = alert.IsAcknowledged,
                Created = alert.Created
            })
            .ToListAsync(cancellationToken);

        return new DashboardSummaryDto
        {
            TotalSensors = await sensorsQuery.CountAsync(cancellationToken),
            ActiveSensors = await sensorsQuery.CountAsync(sensor => sensor.IsActive, cancellationToken),
            InactiveSensors = await sensorsQuery.CountAsync(sensor => !sensor.IsActive, cancellationToken),
            LowBatterySensors = await sensorsQuery.CountAsync(
                sensor => sensor.BatteryLevel.HasValue && sensor.BatteryLevel.Value < LowBatteryThreshold,
                cancellationToken),
            TotalLocations = await locationsQuery.CountAsync(cancellationToken),
            TotalAlerts = await alertsQuery.CountAsync(cancellationToken),
            PendingAlerts = await alertsQuery.CountAsync(alert => !alert.IsAcknowledged, cancellationToken),
            AcknowledgedAlerts = await alertsQuery.CountAsync(alert => alert.IsAcknowledged, cancellationToken),
            AlertsLast24Hours = await alertsQuery.CountAsync(alert => alert.Created >= last24Hours, cancellationToken),
            ReadingsLast24Hours = await readingsQuery.CountAsync(reading => reading.Timestamp >= last24Hours, cancellationToken),
            RecentAlerts = recentAlerts
        };
    }
}
