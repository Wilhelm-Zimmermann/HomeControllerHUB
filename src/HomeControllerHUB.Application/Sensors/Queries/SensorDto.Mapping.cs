using System.Linq.Expressions;
using HomeControllerHUB.Domain.Entities;

namespace HomeControllerHUB.Application.Sensors.Queries;

public partial class SensorDto
{
    public static readonly Expression<Func<Sensor, SensorDto>> Projection = sensor => new SensorDto
    {
        Id = sensor.Id,
        EstablishmentId = sensor.EstablishmentId,
        EstablishmentName = sensor.Establishment.Name ?? string.Empty,
        LocationId = sensor.LocationId,
        LocationName = sensor.Location.Name ?? string.Empty,
        Name = sensor.Name,
        DeviceId = sensor.DeviceId,
        Type = sensor.Type,
        Model = sensor.Model,
        FirmwareVersion = sensor.FirmwareVersion,
        MinThreshold = sensor.MinThreshold,
        MaxThreshold = sensor.MaxThreshold,
        IsActive = sensor.IsActive,
        LastCommunication = sensor.LastCommunication,
        BatteryLevel = sensor.BatteryLevel,
        Created = sensor.Created,
        Modified = sensor.Modified
    };
}
