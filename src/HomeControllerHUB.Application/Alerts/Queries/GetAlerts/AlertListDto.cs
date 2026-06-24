using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;

namespace HomeControllerHUB.Application.Alerts.Queries.GetAlerts;

public class AlertListDto : IPaginatedDto
{
    public Guid Id { get; set; }

    public Guid SensorId { get; set; }
    public string SensorName { get; set; } = null!;
    public string SensorDeviceId { get; set; } = null!;
    public SensorType SensorType { get; set; }
    public string SensorTypeName => SensorType.ToString();

    public Guid LocationId { get; set; }
    public string LocationName { get; set; } = null!;

    public Guid EstablishmentId { get; set; }
    public string? EstablishmentName { get; set; }

    public AlertType Type { get; set; }
    public string TypeName => Type.ToString();
    public string Message { get; set; } = null!;

    public double? MinThreshold { get; set; }
    public double? MaxThreshold { get; set; }

    public bool IsAcknowledged { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public Guid? AcknowledgedById { get; set; }
    public string? AcknowledgedByName { get; set; }

    public DateTime Timestamp { get; set; }
    public DateTime Created { get; set; }
}
