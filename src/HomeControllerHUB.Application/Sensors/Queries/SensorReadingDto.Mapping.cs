using System.Linq.Expressions;
using HomeControllerHUB.Domain.Entities;

namespace HomeControllerHUB.Application.Sensors.Queries;

public partial class SensorReadingDto
{
    public static readonly Expression<Func<SensorReading, SensorReadingDto>> Projection = reading => new SensorReadingDto
    {
        Id = reading.Id,
        SensorId = reading.SensorId,
        SensorName = reading.Sensor.Name,
        SensorTypeName = reading.Sensor.Type.ToString(),
        Timestamp = reading.Timestamp,
        Value = reading.Value,
        Unit = reading.Unit,
        RawData = reading.RawData,
        Created = reading.Created
    };
}
