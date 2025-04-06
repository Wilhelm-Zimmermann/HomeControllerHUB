using HomeControllerHUB.Shared.Normalize;

namespace HomeControllerHUB.Domain.Entities;

public class Sensor : Base
{
    public Guid EstablishmentId { get; set; }
    public virtual Establishment Establishment { get; set; } = null!;
    
    public Guid LocationId { get; set; }
    public virtual Location Location { get; set; } = null!;
    
    public string Name { get; set; } = null!;
    [Normalized(nameof(Name))]
    public string? NormalizedName { get; set; }
    
    public string DeviceId { get; set; } = null!; // Unique identifier for the physical device
    public SensorType Type { get; set; }
    public string Model { get; set; } = null!;
    public string? FirmwareVersion { get; set; }
    public string? ApiKey { get; set; } // API key for sensor authentication
    
    public double? MinThreshold { get; set; } // Minimum value for alerts
    public double? MaxThreshold { get; set; } // Maximum value for alerts
    
    public bool IsActive { get; set; } = true;
    public DateTime LastCommunication { get; set; }
    public double? BatteryLevel { get; set; }
    
    public virtual IList<SensorReading> Readings { get; private set; }
    public virtual IList<SensorAlert> Alerts { get; private set; }
}

public enum SensorType
{
    Temperature,
    Humidity,
    Pressure,
    Light,
    Motion,
    Door,
    Water,
    Smoke,
    Gas,
    Electricity,
    Custom
} 