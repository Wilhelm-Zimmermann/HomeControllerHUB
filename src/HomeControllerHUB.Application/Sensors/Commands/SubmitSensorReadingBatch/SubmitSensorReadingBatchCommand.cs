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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeControllerHUB.Application.Sensors.Commands.SubmitSensorReadingBatch;

// Request class for submitting multiple sensor readings in a single call
public class SubmitSensorReadingBatchCommand : IRequest
{
    public string DeviceId { get; set; } = null!;
    public string? ApiKey { get; set; }
    public List<SensorReadingDto> Readings { get; set; } = new List<SensorReadingDto>();
}

public class SensorReadingDto
{
    public double Value { get; set; }
    public string? Unit { get; set; }
    public DateTime? Timestamp { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

public class SubmitSensorReadingBatchCommandValidator : AbstractValidator<SubmitSensorReadingBatchCommand>
{
    public SubmitSensorReadingBatchCommandValidator()
    {
        RuleFor(x => x.DeviceId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Readings).NotEmpty().WithMessage("At least one reading must be provided");
        RuleForEach(x => x.Readings).ChildRules(reading =>
        {
            reading.RuleFor(r => r.Unit).MaximumLength(20);
        });
    }
}

public class SubmitSensorReadingBatchCommandHandler : IRequestHandler<SubmitSensorReadingBatchCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly ISharedResource _sharedResource;

    public SubmitSensorReadingBatchCommandHandler(
        ApplicationDbContext context,
        ISharedResource sharedResource)
    {
        _context = context;
        _sharedResource = sharedResource;
    }

    public async Task Handle(SubmitSensorReadingBatchCommand request, CancellationToken cancellationToken)
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

        var alerts = new List<SensorAlert>();
        var readings = new List<SensorReading>();

        // Process each reading in the batch
        foreach (var readingDto in request.Readings)
        {
            var timestamp = readingDto.Timestamp ?? DateTime.UtcNow;
            
            // Create the sensor reading
            var reading = new SensorReading
            {
                SensorId = sensor.Id,
                Value = readingDto.Value,
                Unit = readingDto.Unit,
                Timestamp = timestamp,
                Metadata = readingDto.Metadata
            };
            
            readings.Add(reading);

            // Check for threshold violations and create alerts if needed
            if (sensor.MinThreshold.HasValue && readingDto.Value < sensor.MinThreshold.Value)
            {
                var alert = new SensorAlert
                {
                    SensorId = sensor.Id,
                    Type = AlertType.ThresholdBelowMinimum,
                    Message = _sharedResource.Message("SensorValueBelowThreshold"),
                    Timestamp = timestamp,
                    IsAcknowledged = false
                };
                
                alerts.Add(alert);
            }
            else if (sensor.MaxThreshold.HasValue && readingDto.Value > sensor.MaxThreshold.Value)
            {
                var alert = new SensorAlert
                {
                    SensorId = sensor.Id,
                    Type = AlertType.ThresholdExceeded,
                    Message = _sharedResource.Message("SensorValueAboveThreshold"),
                    Timestamp = timestamp,
                    IsAcknowledged = false
                };
                
                alerts.Add(alert);
            }
        }

        // Add all readings and alerts to the context
        await _context.SensorReadings.AddRangeAsync(readings, cancellationToken);
        
        if (alerts.Any())
        {
            await _context.SensorAlerts.AddRangeAsync(alerts, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
} 