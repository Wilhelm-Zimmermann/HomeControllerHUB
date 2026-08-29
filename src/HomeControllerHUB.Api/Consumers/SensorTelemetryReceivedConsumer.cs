using HomeControllerHUB.Application.Sensors.Commands.ProcessSensorReading;
using HomeControllerHUB.Domain.Messages;
using MassTransit;
using MediatR;

namespace HomeControllerHUB.Api.Consumers;

public class SensorTelemetryReceivedConsumer : IConsumer<SensorTelemetryReceivedMessage>
{
    private readonly ISender _sender;
    private readonly ILogger<SensorTelemetryReceivedConsumer> _logger;

    public SensorTelemetryReceivedConsumer(
        ISender sender,
        ILogger<SensorTelemetryReceivedConsumer> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SensorTelemetryReceivedMessage> context)
    {
        var message = context.Message;
        using var scope = BeginCorrelationScope(message.CorrelationId);

        var response = await _sender.Send(new ProcessSensorReadingCommand
        {
            SensorId = message.SensorId,
            DeviceId = message.DeviceId,
            MessageId = message.MessageId,
            Timestamp = message.Timestamp,
            Value = message.Value,
            Unit = message.Unit,
            BatteryLevel = message.BatteryLevel,
            RawData = message.RawData,
            CorrelationId = message.CorrelationId
        }, context.CancellationToken);

        _logger.LogInformation(
            "Sensor telemetry consumed for device {DeviceId}, sensor {SensorId}, message {MessageId}, status {Status}, correlation {CorrelationId}",
            message.DeviceId,
            response.SensorId,
            response.MessageId,
            response.Status,
            message.CorrelationId);
    }

    private IDisposable? BeginCorrelationScope(string? correlationId)
    {
        return string.IsNullOrWhiteSpace(correlationId)
            ? null
            : _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId
            });
    }
}
