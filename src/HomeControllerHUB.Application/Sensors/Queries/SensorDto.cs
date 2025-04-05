using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Mappings;

namespace HomeControllerHUB.Application.Sensors.Queries;

public class SensorDto : IMapFrom<Sensor>, IPaginatedDto
{
    public Guid Id { get; set; }
    public Guid EstablishmentId { get; set; }
    public string EstablishmentName { get; set; } = null!;
    
    public Guid LocationId { get; set; }
    public string LocationName { get; set; } = null!;
    
    public string Name { get; set; } = null!;
    public string DeviceId { get; set; } = null!;
    public SensorType Type { get; set; }
    public string TypeName => Type.ToString();
    public string Model { get; set; } = null!;
    public string? FirmwareVersion { get; set; }
    
    public double? MinThreshold { get; set; }
    public double? MaxThreshold { get; set; }
    
    public bool IsActive { get; set; }
    public DateTime LastCommunication { get; set; }
    public double? BatteryLevel { get; set; }
    public string Status => DetermineStatus();
    
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
    
    private string DetermineStatus()
    {
        if (!IsActive)
            return "Inactive";
            
        if (DateTime.UtcNow.Subtract(LastCommunication).TotalHours > 1)
            return "Offline";
            
        if (BatteryLevel.HasValue && BatteryLevel.Value < 20)
            return "Low Battery";
            
        return "Online";
    }
} 