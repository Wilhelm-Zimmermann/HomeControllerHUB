using FluentValidation;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Sensors.Commands.DeleteSensor;

[Authorize(Domain = DomainNames.IoT, Action = SecurityActionType.Delete)]
public class DeleteSensorCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteSensorCommandValidator : AbstractValidator<DeleteSensorCommand>
{
    public DeleteSensorCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class DeleteSensorCommandHandler : IRequestHandler<DeleteSensorCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly ISharedResource _sharedResource;

    public DeleteSensorCommandHandler(
        ApplicationDbContext context,
        ISharedResource sharedResource)
    {
        _context = context;
        _sharedResource = sharedResource;
    }

    public async Task Handle(DeleteSensorCommand request, CancellationToken cancellationToken)
    {
        var sensor = await _context.Sensors
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (sensor == null)
        {
            throw new AppError(
                StatusCodes.Status404NotFound,
                _sharedResource.NotFoundMessage("Sensor"),
                _sharedResource.Message("SensorNotFound"));
        }

        // Check for sensor readings before deletion
        var hasReadings = await _context.SensorReadings
            .AnyAsync(x => x.SensorId == request.Id, cancellationToken);

        if (hasReadings)
        {
            // Option 1: Prevent deletion if sensor has readings
            throw new AppError(
                StatusCodes.Status400BadRequest,
                _sharedResource.Message("DeleteFailed"),
                _sharedResource.Message("SensorHasReadingsCannotBeDeleted"));

            // Option 2: Cascade delete readings (uncomment if this is the desired behavior)
            // var readings = await _context.SensorReadings
            //     .Where(x => x.SensorId == request.Id)
            //     .ToListAsync(cancellationToken);
            // _context.SensorReadings.RemoveRange(readings);
        }

        // Check for sensor alerts before deletion
        var hasAlerts = await _context.SensorAlerts
            .AnyAsync(x => x.SensorId == request.Id, cancellationToken);

        if (hasAlerts)
        {
            // Option 1: Prevent deletion if sensor has alerts
            throw new AppError(
                StatusCodes.Status400BadRequest,
                _sharedResource.Message("DeleteFailed"),
                _sharedResource.Message("SensorHasAlertsCannotBeDeleted"));

            // Option 2: Cascade delete alerts (uncomment if this is the desired behavior)
            // var alerts = await _context.SensorAlerts
            //     .Where(x => x.SensorId == request.Id)
            //     .ToListAsync(cancellationToken);
            // _context.SensorAlerts.RemoveRange(alerts);
        }

        _context.Sensors.Remove(sensor);
        await _context.SaveChangesAsync(cancellationToken);
    }
} 