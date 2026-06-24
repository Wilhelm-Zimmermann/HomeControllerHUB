namespace HomeControllerHUB.Application.Dashboard.Queries.GetDashboardSummary;

public class DashboardSummaryDto
{
    public int TotalSensors { get; set; }
    public int ActiveSensors { get; set; }
    public int InactiveSensors { get; set; }
    public int LowBatterySensors { get; set; }

    public int TotalLocations { get; set; }

    public int TotalAlerts { get; set; }
    public int PendingAlerts { get; set; }
    public int AcknowledgedAlerts { get; set; }
    public int AlertsLast24Hours { get; set; }

    public int ReadingsLast24Hours { get; set; }

    public List<DashboardRecentAlertDto> RecentAlerts { get; set; } = [];
}
