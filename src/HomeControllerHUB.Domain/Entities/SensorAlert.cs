namespace HomeControllerHUB.Domain.Entities;

public class SensorAlert : Base
{
    public Guid SensorId { get; set; }
    public virtual Sensor Sensor { get; set; } = null!;
    
    public AlertType Type { get; set; }
    public string Message { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public bool IsAcknowledged { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public Guid? AcknowledgedById { get; set; }
    public virtual ApplicationUser? AcknowledgedBy { get; set; }
}

public enum AlertType
{
    ThresholdExceeded,
    ThresholdBelowMinimum,
    DeviceOffline,
    BatteryLow,
    Error
} 