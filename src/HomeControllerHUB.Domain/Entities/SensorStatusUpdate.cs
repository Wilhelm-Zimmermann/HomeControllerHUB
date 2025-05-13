namespace HomeControllerHUB.Domain.Entities;

public class SensorStatusUpdate : Base
{
    public Guid SensorId { get; set; }
    public virtual Sensor Sensor { get; set; } = null!;
    
    public DateTime Timestamp { get; set; }
    public string? Status { get; set; }
    public double? BatteryLevel { get; set; }
    public string? SignalStrength { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
} 