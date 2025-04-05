using FluentValidation;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Sensors.Commands.UpdateSensor;

[Authorize(Domain = DomainNames.IoT, Action = SecurityActionType.Update)]
public class UpdateSensorCommand : IRequest
{
    public Guid Id { get; set; }
    public Guid EstablishmentId { get; set; }
    public Guid LocationId { get; set; }
    public string Name { get; set; } = null!;
    public string DeviceId { get; set; } = null!;
    public SensorType Type { get; set; }
    public string Model { get; set; } = null!;
    public string? FirmwareVersion { get; set; }
    public string? ApiKey { get; set; }
    public double? MinThreshold { get; set; }
    public double? MaxThreshold { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateSensorCommandValidator : AbstractValidator<UpdateSensorCommand>
{
    public UpdateSensorCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.EstablishmentId).NotEmpty();
        RuleFor(x => x.LocationId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        RuleFor(x => x.DeviceId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FirmwareVersion).MaximumLength(50);
        RuleFor(x => x.ApiKey).MaximumLength(100);
        
        // Optional validation for threshold values
        When(x => x.MinThreshold.HasValue && x.MaxThreshold.HasValue, () => {
            RuleFor(x => x.MinThreshold!.Value).LessThan(x => x.MaxThreshold!.Value)
                .WithMessage("Minimum threshold must be less than maximum threshold");
        });
    }
}

public class UpdateSensorCommandHandler : IRequestHandler<UpdateSensorCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly ISharedResource _sharedResource;

    public UpdateSensorCommandHandler(
        ApplicationDbContext context,
        ISharedResource sharedResource)
    {
        _context = context;
        _sharedResource = sharedResource;
    }

    public async Task Handle(UpdateSensorCommand request, CancellationToken cancellationToken)
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

        // Check if establishment exists
        var establishmentExists = await _context.Establishments
            .AnyAsync(x => x.Id == request.EstablishmentId, cancellationToken);

        if (!establishmentExists)
        {
            throw new AppError(
                StatusCodes.Status400BadRequest,
                _sharedResource.Message("ValidationFailed"),
                _sharedResource.Message("EstablishmentNotFound"));
        }

        // Check if location exists
        var locationExists = await _context.Locations
            .AnyAsync(x => x.Id == request.LocationId, cancellationToken);

        if (!locationExists)
        {
            throw new AppError(
                StatusCodes.Status400BadRequest,
                _sharedResource.Message("ValidationFailed"),
                _sharedResource.Message("LocationNotFound"));
        }

        // Check if DeviceId is already in use by another sensor
        var existingDeviceId = await _context.Sensors
            .FirstOrDefaultAsync(x => x.DeviceId == request.DeviceId && x.Id != request.Id, cancellationToken);

        if (existingDeviceId != null)
        {
            throw new AppError(
                StatusCodes.Status400BadRequest,
                _sharedResource.Message("ValidationFailed"),
                _sharedResource.Message("DeviceIdAlreadyInUse"));
        }

        // Update sensor properties
        sensor.EstablishmentId = request.EstablishmentId;
        sensor.LocationId = request.LocationId;
        sensor.Name = request.Name;
        sensor.DeviceId = request.DeviceId;
        sensor.Type = request.Type;
        sensor.Model = request.Model;
        sensor.FirmwareVersion = request.FirmwareVersion;
        sensor.ApiKey = request.ApiKey;
        sensor.MinThreshold = request.MinThreshold;
        sensor.MaxThreshold = request.MaxThreshold;
        sensor.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
    }
} 