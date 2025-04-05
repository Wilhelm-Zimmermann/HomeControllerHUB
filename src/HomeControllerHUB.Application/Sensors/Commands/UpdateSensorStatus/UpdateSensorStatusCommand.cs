using FluentValidation;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HomeControllerHUB.Application.Sensors.Commands.UpdateSensorStatus;

public class UpdateSensorStatusCommand : IRequest
{
    public string DeviceId { get; set; } = null!;
    public string? ApiKey { get; set; }
    public bool IsActive { get; set; }
    public string? Status { get; set; }
    public double? BatteryLevel { get; set; }
    public string? FirmwareVersion { get; set; }
    public string? SignalStrength { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

public class UpdateSensorStatusCommandValidator : AbstractValidator<UpdateSensorStatusCommand>
{
    public UpdateSensorStatusCommandValidator()
    {
        RuleFor(x => x.DeviceId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Status).MaximumLength(100);
        RuleFor(x => x.FirmwareVersion).MaximumLength(50);
        RuleFor(x => x.SignalStrength).MaximumLength(20);
        RuleFor(x => x.BatteryLevel).InclusiveBetween(0, 100)
            .When(x => x.BatteryLevel.HasValue);
    }
}

public class UpdateSensorStatusCommandHandler : IRequestHandler<UpdateSensorStatusCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly ISharedResource _sharedResource;

    public UpdateSensorStatusCommandHandler(
        ApplicationDbContext context,
        ISharedResource sharedResource)
    {
        _context = context;
        _sharedResource = sharedResource;
    }

    public async Task Handle(UpdateSensorStatusCommand request, CancellationToken cancellationToken)
    {
        // Verify the sensor exists
        var sensor = await _context.Sensors
            .FirstOrDefaultAsync(s => s.DeviceId == request.DeviceId, cancellationToken);

        if (sensor == null)
        {
            throw new AppError(
                StatusCodes.Status404NotFound,
                _sharedResource.NotFoundMessage("Sensor"),
                _sharedResource.Message("SensorNotFound"));
        }

        // Verify the API key if provided
        if (!string.IsNullOrEmpty(sensor.ApiKey) && sensor.ApiKey != request.ApiKey)
        {
            throw new AppError(
                StatusCodes.Status401Unauthorized,
                _sharedResource.Message("AuthenticationFailed"),
                _sharedResource.Message("InvalidApiKey"));
        }

        // Update sensor status
        sensor.IsActive = request.IsActive;
        sensor.LastCommunication = DateTime.UtcNow;
        
        // Update firmware version if provided
        if (!string.IsNullOrEmpty(request.FirmwareVersion))
        {
            sensor.FirmwareVersion = request.FirmwareVersion;
        }
        
        // Create a status update record with additional details
        var statusUpdate = new SensorStatusUpdate
        {
            SensorId = sensor.Id,
            Timestamp = DateTime.UtcNow,
            Status = request.Status,
            BatteryLevel = request.BatteryLevel,
            SignalStrength = request.SignalStrength,
            Metadata = request.Metadata
        };
        
        await _context.SensorStatusUpdates.AddAsync(statusUpdate, cancellationToken);
        
        // Create a low battery alert if battery level is critical (below 15%)
        if (request.BatteryLevel.HasValue && request.BatteryLevel < 15)
        {
            var alert = new SensorAlert
            {
                SensorId = sensor.Id,
                Type = AlertType.BatteryLow,
                Message = _sharedResource.Message("SensorLowBattery"),
                Timestamp = DateTime.UtcNow,
                IsAcknowledged = false
            };
            
            await _context.SensorAlerts.AddAsync(alert, cancellationToken);
        }
        
        await _context.SaveChangesAsync(cancellationToken);
    }
} 