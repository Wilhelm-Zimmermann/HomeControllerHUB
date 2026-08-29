using FluentValidation;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeControllerHUB.Application.Sensors.Commands.ProcessSensorReading;

public class ProcessSensorReadingCommand : IRequest<ProcessSensorReadingResponse>
{
    public Guid SensorId { get; set; }
    public string DeviceId { get; set; } = null!;
    public string MessageId { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public string? Unit { get; set; }
    public double? BatteryLevel { get; set; }
    public string? RawData { get; set; }
    public string? CorrelationId { get; set; }
}

public class ProcessSensorReadingResponse
{
    public Guid SensorId { get; set; }
    public Guid ReadingId { get; set; }
    public string MessageId { get; set; } = null!;
    public string Status { get; set; } = null!;
    public bool AlertCreated { get; set; }
    public Guid? AlertId { get; set; }
}

public static class ProcessSensorReadingStatus
{
    public const string Processed = "Processed";
    public const string Duplicate = "Duplicate";
}

public class ProcessSensorReadingCommandValidator : AbstractValidator<ProcessSensorReadingCommand>
{
    public ProcessSensorReadingCommandValidator()
    {
        RuleFor(x => x.DeviceId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MessageId).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Unit).MaximumLength(20);
        RuleFor(x => x.BatteryLevel).InclusiveBetween(0, 100)
            .When(x => x.BatteryLevel.HasValue);
        RuleFor(x => x.RawData).MaximumLength(1000);
    }
}

public class ProcessSensorReadingCommandHandler : IRequestHandler<ProcessSensorReadingCommand, ProcessSensorReadingResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly ISharedResource _sharedResource;
    private readonly ILogger<ProcessSensorReadingCommandHandler> _logger;

    public ProcessSensorReadingCommandHandler(
        ApplicationDbContext context,
        ISharedResource sharedResource,
        ILogger<ProcessSensorReadingCommandHandler> logger)
    {
        _context = context;
        _sharedResource = sharedResource;
        _logger = logger;
    }

    public async Task<ProcessSensorReadingResponse> Handle(ProcessSensorReadingCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await new ProcessSensorReadingCommandValidator().ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new AppError(
                StatusCodes.Status400BadRequest,
                "Validation failed",
                string.Join("; ", validationResult.Errors.Select(error => error.ErrorMessage)));
        }

        var sensor = await _context.Sensors
            .FirstOrDefaultAsync(
                s => s.DeviceId == request.DeviceId,
                cancellationToken);

        if (sensor is null)
        {
            _logger.LogWarning(
                "Sensor telemetry processing rejected for missing sensor {SensorId}, device {DeviceId}, message {MessageId} and correlation {CorrelationId}",
                request.SensorId,
                request.DeviceId,
                request.MessageId,
                request.CorrelationId);

            throw new AppError(
                StatusCodes.Status404NotFound,
                _sharedResource.NotFoundMessage("Sensor"),
                _sharedResource.Message("SensorNotFound"));
        }

        var existingReading = await _context.SensorReadings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                reading => reading.SensorId == sensor.Id && reading.MessageId == request.MessageId,
                cancellationToken);

        if (existingReading is not null)
        {
            return CreateDuplicateResponse(sensor.Id, existingReading.Id, request.MessageId);
        }

        var timestamp = request.Timestamp == default
            ? DateTime.UtcNow
            : request.Timestamp;

        var reading = new SensorReading
        {
            SensorId = sensor.Id,
            MessageId = request.MessageId,
            Timestamp = timestamp,
            Value = request.Value,
            Unit = request.Unit,
            RawData = request.RawData
        };

        sensor.LastCommunication = DateTime.UtcNow;

        if (request.BatteryLevel.HasValue)
        {
            sensor.BatteryLevel = request.BatteryLevel;
        }

        await _context.SensorReadings.AddAsync(reading, cancellationToken);

        var alert = await CreateThresholdAlertAsync(sensor, reading, cancellationToken);
        if (alert is not null)
        {
            await _context.SensorAlerts.AddAsync(alert, cancellationToken);
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Sensor reading processed. SensorId={SensorId}, ReadingId={ReadingId}, MessageId={MessageId}",
                sensor.Id,
                reading.Id,
                request.MessageId
            );
        }
        catch (DbUpdateException)
        {
            _context.Entry(reading).State = EntityState.Detached;
            if (alert is not null)
            {
                _context.Entry(alert).State = EntityState.Detached;
            }

            var duplicateReading = await _context.SensorReadings
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    duplicate => duplicate.SensorId == sensor.Id && duplicate.MessageId == request.MessageId,
                    cancellationToken);

            if (duplicateReading is not null)
            {
                return CreateDuplicateResponse(sensor.Id, duplicateReading.Id, request.MessageId);
            }

            throw;
        }

        return new ProcessSensorReadingResponse
        {
            SensorId = sensor.Id,
            ReadingId = reading.Id,
            MessageId = request.MessageId,
            Status = ProcessSensorReadingStatus.Processed,
            AlertCreated = alert is not null,
            AlertId = alert?.Id
        };
    }

    private async Task<SensorAlert?> CreateThresholdAlertAsync(
        Sensor sensor,
        SensorReading reading,
        CancellationToken cancellationToken)
    {
        var alertType = GetThresholdAlertType(sensor, reading.Value);
        if (alertType is null)
        {
            return null;
        }

        var hasPendingAlert = await _context.SensorAlerts.AnyAsync(
            alert => alert.SensorId == sensor.Id
                     && alert.Type == alertType.Value
                     && !alert.IsAcknowledged,
            cancellationToken);

        if (hasPendingAlert)
        {
            return null;
        }

        return new SensorAlert
        {
            SensorId = sensor.Id,
            Type = alertType.Value,
            Message = alertType.Value == AlertType.ThresholdBelowMinimum
                ? _sharedResource.Message("SensorValueBelowThreshold")
                : _sharedResource.Message("SensorValueAboveThreshold"),
            Timestamp = reading.Timestamp,
            IsAcknowledged = false
        };
    }

    private static AlertType? GetThresholdAlertType(Sensor sensor, double value)
    {
        if (sensor.MinThreshold.HasValue && value < sensor.MinThreshold.Value)
        {
            return AlertType.ThresholdBelowMinimum;
        }

        if (sensor.MaxThreshold.HasValue && value > sensor.MaxThreshold.Value)
        {
            return AlertType.ThresholdExceeded;
        }

        return null;
    }

    private static ProcessSensorReadingResponse CreateDuplicateResponse(Guid sensorId, Guid readingId, string messageId)
    {
        return new ProcessSensorReadingResponse
        {
            SensorId = sensorId,
            ReadingId = readingId,
            MessageId = messageId,
            Status = ProcessSensorReadingStatus.Duplicate,
            AlertCreated = false
        };
    }
}
