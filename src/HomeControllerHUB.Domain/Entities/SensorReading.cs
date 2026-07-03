namespace HomeControllerHUB.Domain.Entities;

public class SensorReading : Base
{
    public Guid SensorId { get; set; }
    public virtual Sensor Sensor { get; set; } = null!;

    public string? MessageId { get; set; }
    
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public string? Unit { get; set; } // °C, %, ppm, etc.
    
    public string? RawData { get; set; } // JSON or other format for additional data
    public Dictionary<string, string>? Metadata { get; set; }
} 
