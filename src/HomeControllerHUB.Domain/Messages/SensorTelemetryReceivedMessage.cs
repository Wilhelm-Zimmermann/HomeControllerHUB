namespace HomeControllerHUB.Domain.Messages;

public class SensorTelemetryReceivedMessage
{
    public Guid SensorId { get; set; }
    public string DeviceId { get; set; } = null!;
    public string MessageId { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public string? Unit { get; set; }
    public double? BatteryLevel { get; set; }
    public string? RawData { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string? CorrelationId { get; set; }
}
