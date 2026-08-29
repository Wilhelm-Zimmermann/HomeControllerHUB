using System.Text.Json;
using HomeControllerHUB.Application.Sensors.Commands.IngestSensorReading;
using HomeControllerHUB.Infra.Mosquitto.Interfaces;
using MediatR;

namespace HomeControllerHUB.Api.Mosquitto;

public class SensorTelemetryReceivedConsumer : IBrokerConsumer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ISender _sender;
    private readonly ILogger<SensorTelemetryReceivedConsumer> _logger;

    public SensorTelemetryReceivedConsumer(
        ISender sender,
        ILogger<SensorTelemetryReceivedConsumer> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public string Topic => "sensor/telemetry";

    public async Task ExecuteTopicAsync(string payload, CancellationToken cancellationToken)
    {
        SensorTelemetryPayload? telemetry;
        try
        {
            telemetry = JsonSerializer.Deserialize<SensorTelemetryPayload>(payload, SerializerOptions);
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(exception, "Invalid MQTT sensor telemetry payload received");
            throw new InvalidOperationException("sensor/telemetry returned an invalid message", exception);
        }

        if (telemetry is null)
        {
            _logger.LogWarning("Empty MQTT sensor telemetry payload received");
            throw new InvalidOperationException("sensor/telemetry returned an invalid message");
        }

        await _sender.Send(new IngestSensorReadingCommand
        {
            ApiKey = telemetry.ApiKey,
            DeviceId = telemetry.DeviceId,
            MessageId = telemetry.MessageId,
            Timestamp = telemetry.Timestamp,
            Value = telemetry.Value,
            Unit = telemetry.Unit,
            BatteryLevel = telemetry.BatteryLevel,
            RawData = telemetry.RawData,
            CorrelationId = telemetry.CorrelationId
        }, cancellationToken);

        _logger.LogInformation(
            "MQTT sensor telemetry accepted for device {DeviceId} and message {MessageId}",
            telemetry.DeviceId,
            telemetry.MessageId);
    }

    private sealed class SensorTelemetryPayload
    {
        public string ApiKey { get; set; } = null!;
        public string DeviceId { get; set; } = null!;
        public string MessageId { get; set; } = null!;
        public DateTime? Timestamp { get; set; }
        public double? Value { get; set; }
        public string? Unit { get; set; }
        public double? BatteryLevel { get; set; }
        public string? RawData { get; set; }
        public string? CorrelationId { get; set; }
    }
}
