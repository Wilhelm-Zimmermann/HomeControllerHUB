using FluentValidation;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.Sensors.Commands.SubmitSensorReading;

public class SubmitSensorReadingCommand : IRequest
{
    public string DeviceId { get; set; } = null!;
    public string? ApiKey { get; set; }
    public double Value { get; set; }
    public string? Unit { get; set; }
    public DateTime? Timestamp { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

public class SubmitSensorReadingCommandValidator : AbstractValidator<SubmitSensorReadingCommand>
{
    public SubmitSensorReadingCommandValidator()
    {
        RuleFor(x => x.DeviceId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Unit).MaximumLength(20);
    }
}

public class SubmitSensorReadingCommandHandler : IRequestHandler<SubmitSensorReadingCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly ISharedResource _sharedResource;

    public SubmitSensorReadingCommandHandler(
        ApplicationDbContext context,
        ISharedResource sharedResource)
    {
        _context = context;
        _sharedResource = sharedResource;
    }

    public async Task Handle(SubmitSensorReadingCommand request, CancellationToken cancellationToken)
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

        // Check if the sensor is active
        if (!sensor.IsActive)
        {
            return; // Silently ignore readings from inactive sensors
        }

        // Create and save the sensor reading
        var reading = new SensorReading
        {
            SensorId = sensor.Id,
            Value = request.Value,
            Unit = request.Unit,
            Timestamp = request.Timestamp ?? DateTime.UtcNow,
            Metadata = request.Metadata
        };

        _context.SensorReadings.Add(reading);

        // Check for threshold violations and create alerts if needed
        if (sensor.MinThreshold.HasValue && request.Value < sensor.MinThreshold.Value)
        {
            var alert = new SensorAlert
            {
                SensorId = sensor.Id,
                Type = AlertType.ThresholdBelowMinimum,
                Message = _sharedResource.Message("SensorValueBelowThreshold"),
                Timestamp = reading.Timestamp,
                IsAcknowledged = false
            };

            _context.SensorAlerts.Add(alert);
        }
        else if (sensor.MaxThreshold.HasValue && request.Value > sensor.MaxThreshold.Value)
        {
            var alert = new SensorAlert
            {
                SensorId = sensor.Id,
                Type = AlertType.ThresholdExceeded,
                Message = _sharedResource.Message("SensorValueAboveThreshold"),
                Timestamp = reading.Timestamp,
                IsAcknowledged = false
            };

            _context.SensorAlerts.Add(alert);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
} 