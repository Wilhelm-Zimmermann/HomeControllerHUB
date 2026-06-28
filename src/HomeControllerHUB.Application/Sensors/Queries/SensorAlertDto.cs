using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
namespace HomeControllerHUB.Application.Sensors.Queries;

public partial class SensorAlertDto : IPaginatedDto
{
    public Guid Id { get; set; }
    public Guid SensorId { get; set; }
    public string SensorName { get; set; } = null!;
    public string SensorTypeName { get; set; } = null!;
    
    public AlertType Type { get; set; }
    public string TypeName => Type.ToString();
    public string Message { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public bool IsAcknowledged { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public Guid? AcknowledgedById { get; set; }
    public string? AcknowledgedByName { get; set; }
    
    public DateTime Created { get; set; }
} 
