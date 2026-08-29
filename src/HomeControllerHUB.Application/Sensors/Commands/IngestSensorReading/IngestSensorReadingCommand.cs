using FluentValidation;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Messages;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra.DatabaseContext;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static MassTransit.Monitoring.Performance.BuiltInCounters;

namespace HomeControllerHUB.Application.Sensors.Commands.IngestSensorReading;

public class IngestSensorReadingCommand : IRequest<IngestSensorReadingResponse>
{
    public string ApiKey { get; set; } = null!;
    public string MessageId { get; set; } = null!;
    public string DeviceId { get; set; } = null!;
    public DateTime? Timestamp { get; set; }
    public double? Value { get; set; }
    public string? Unit { get; set; }
    public double? BatteryLevel { get; set; }
    public string? RawData { get; set; }
    public string? CorrelationId { get; set; }
}

public class IngestSensorReadingResponse
{
    public Guid SensorId { get; set; }
    public string MessageId { get; set; } = null!;
    public string Status { get; set; } = null!;
}

public static class IngestSensorReadingStatus
{
    public const string Queued = "Queued";
    public const string Duplicate = "Duplicate";
}

public class IngestSensorReadingCommandValidator : AbstractValidator<IngestSensorReadingCommand>
{
    public IngestSensorReadingCommandValidator()
    {
        RuleFor(x => x.ApiKey).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MessageId).NotEmpty().MaximumLength(150);
        RuleFor(x => x.DeviceId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Value).NotNull();
        RuleFor(x => x.Unit).MaximumLength(20);
        RuleFor(x => x.BatteryLevel).InclusiveBetween(0, 100)
            .When(x => x.BatteryLevel.HasValue);
        RuleFor(x => x.RawData).MaximumLength(1000);
    }
}

public class IngestSensorReadingCommandHandler : IRequestHandler<IngestSensorReadingCommand, IngestSensorReadingResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly ISharedResource _sharedResource;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<IngestSensorReadingCommandHandler> _logger;

    public IngestSensorReadingCommandHandler(
        ApplicationDbContext context,
        ISharedResource sharedResource,
        IPublishEndpoint publishEndpoint,
        ILogger<IngestSensorReadingCommandHandler> logger)
    {
        _context = context;
        _sharedResource = sharedResource;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<IngestSensorReadingResponse> Handle(IngestSensorReadingCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await new IngestSensorReadingCommandValidator().ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new AppError(
                StatusCodes.Status400BadRequest,
                "Validation failed",
                string.Join("; ", validationResult.Errors.Select(error => error.ErrorMessage)));
        }

        var sensor = await _context.Sensors
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.DeviceId == request.DeviceId, cancellationToken);

        if (sensor is null)
        {
            _logger.LogWarning(
                "Sensor telemetry queueing rejected for unknown device {DeviceId} and message {MessageId}",
                request.DeviceId,
                request.MessageId);

            throw new AppError(
                StatusCodes.Status404NotFound,
                _sharedResource.NotFoundMessage("Sensor"),
                _sharedResource.Message("SensorNotFound"));
        }

        if (string.IsNullOrWhiteSpace(sensor.ApiKey) || sensor.ApiKey != request.ApiKey)
        {
            _logger.LogWarning(
                "Sensor telemetry queueing rejected by API key validation for device {DeviceId}, sensor {SensorId} and message {MessageId}",
                request.DeviceId,
                sensor.Id,
                request.MessageId);

            throw new AppError(
                StatusCodes.Status401Unauthorized,
                _sharedResource.Message("AuthenticationFailed"),
                _sharedResource.Message("InvalidApiKey"));
        }

        if (!sensor.IsActive)
        {
            _logger.LogWarning(
                "Sensor telemetry queueing rejected because sensor {SensorId} for device {DeviceId} is inactive and message {MessageId}",
                sensor.Id,
                request.DeviceId,
                request.MessageId);

            throw new AppError(
                StatusCodes.Status403Forbidden,
                "Sensor inactive",
                "Sensor is inactive");
        }

        var alreadyProcessed = await _context.SensorReadings
            .AsNoTracking()
            .AnyAsync(
                reading => reading.SensorId == sensor.Id && reading.MessageId == request.MessageId,
                cancellationToken);

        if (alreadyProcessed)
        {
            _logger.LogInformation(
                "Sensor telemetry message {MessageId} for sensor {SensorId} is already processed",
                request.MessageId,
                sensor.Id);

            return new IngestSensorReadingResponse
            {
                SensorId = sensor.Id,
                MessageId = request.MessageId,
                Status = IngestSensorReadingStatus.Duplicate
            };
        }

        var telemetryMessage = new SensorTelemetryReceivedMessage
        {
            SensorId = sensor.Id,
            DeviceId = request.DeviceId,
            MessageId = request.MessageId,
            Timestamp = request.Timestamp ?? DateTime.UtcNow,
            Value = request.Value!.Value,
            Unit = request.Unit,
            BatteryLevel = request.BatteryLevel,
            RawData = request.RawData,
            ReceivedAt = DateTime.UtcNow,
            CorrelationId = request.CorrelationId
        };

        await _publishEndpoint.Publish(telemetryMessage, cancellationToken);

        _logger.LogInformation(
            "Sensor telemetry message {MessageId} for device {DeviceId} and sensor {SensorId} queued with correlation {CorrelationId}",
            request.MessageId,
            request.DeviceId,
            sensor.Id,
            request.CorrelationId);

        return new IngestSensorReadingResponse
        {
            SensorId = sensor.Id,
            MessageId = request.MessageId,
            Status = IngestSensorReadingStatus.Queued
        };
    }
}
