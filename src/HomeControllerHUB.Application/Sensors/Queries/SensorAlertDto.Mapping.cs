using System.Linq.Expressions;
using HomeControllerHUB.Domain.Entities;

namespace HomeControllerHUB.Application.Sensors.Queries;

public partial class SensorAlertDto
{
    public static readonly Expression<Func<SensorAlert, SensorAlertDto>> Projection = alert => new SensorAlertDto
    {
        Id = alert.Id,
        SensorId = alert.SensorId,
        SensorName = alert.Sensor.Name,
        SensorTypeName = alert.Sensor.Type.ToString(),
        Type = alert.Type,
        Message = alert.Message,
        Timestamp = alert.Timestamp,
        IsAcknowledged = alert.IsAcknowledged,
        AcknowledgedAt = alert.AcknowledgedAt,
        AcknowledgedById = alert.AcknowledgedById,
        AcknowledgedByName = alert.AcknowledgedBy != null ? alert.AcknowledgedBy.Name : null,
        Created = alert.Created
    };
}
