using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Mappings;

namespace HomeControllerHUB.Application.Sensors.Queries;

public class SensorReadingDto : IMapFrom<SensorReading>, IPaginatedDto
{
    public Guid Id { get; set; }
    public Guid SensorId { get; set; }
    public string SensorName { get; set; } = null!;
    public string SensorTypeName { get; set; } = null!;
    
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public string? Unit { get; set; }
    public string? RawData { get; set; }
    
    public DateTime Created { get; set; }
} 