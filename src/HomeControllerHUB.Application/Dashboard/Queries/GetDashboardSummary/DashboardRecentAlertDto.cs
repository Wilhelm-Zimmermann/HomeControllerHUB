using HomeControllerHUB.Domain.Entities;

namespace HomeControllerHUB.Application.Dashboard.Queries.GetDashboardSummary;

public class DashboardRecentAlertDto
{
    public Guid Id { get; set; }
    public AlertType Type { get; set; }
    public string TypeName => Type.ToString();
    public string? Message { get; set; }

    public Guid? SensorId { get; set; }
    public string? SensorName { get; set; }
    public string? SensorDeviceId { get; set; }

    public string? LocationName { get; set; }

    public bool IsAcknowledged { get; set; }
    public DateTime Created { get; set; }
}
