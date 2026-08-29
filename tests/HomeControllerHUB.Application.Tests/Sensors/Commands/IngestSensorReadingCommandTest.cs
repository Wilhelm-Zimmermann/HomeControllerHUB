using FluentAssertions;
using HomeControllerHUB.Application.Sensors.Commands.IngestSensorReading;
using HomeControllerHUB.Application.Sensors.Commands.ProcessSensorReading;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Messages;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HomeControllerHUB.Application.Tests.Sensors;

public class IngestSensorReadingCommandTest : TestConfigs
{
    private const string ApiKey = "sensor-api-key";
    private readonly Mock<ISharedResource> _resourceMock;

    public IngestSensorReadingCommandTest()
    {
        _resourceMock = new Mock<ISharedResource>();
        _resourceMock.Setup(r => r.Message(It.IsAny<string>())).Returns((string key) => key);
        _resourceMock.Setup(r => r.NotFoundMessage(It.IsAny<string>())).Returns((string entity) => $"{entity} not found");
    }

    [Fact]
    public async Task Ingest_Should_PublishMessageAndReturnQueued_WhenPayloadIsValid()
    {
        var sensor = await CreateSensorAsync();
        var publishEndpointMock = CreatePublishEndpointMock(out var queuedMessage);
        var handler = CreateIngestHandler(publishEndpointMock.Object);

        var response = await handler.Handle(CreateIngestCommand(sensor.DeviceId), CancellationToken.None);

        response.Status.Should().Be(IngestSensorReadingStatus.Queued);
        response.SensorId.Should().Be(sensor.Id);
        response.MessageId.Should().Be("message-1");
        queuedMessage.Value.Should().NotBeNull();
        queuedMessage.Value!.SensorId.Should().Be(sensor.Id);
        queuedMessage.Value.DeviceId.Should().Be(sensor.DeviceId);
        queuedMessage.Value.MessageId.Should().Be("message-1");
        queuedMessage.Value.CorrelationId.Should().Be("test-correlation");
        _context.SensorReadings.Should().BeEmpty();
    }

    [Fact]
    public async Task Ingest_Should_NotPublishMessage_WhenApiKeyIsInvalid()
    {
        var sensor = await CreateSensorAsync();
        var publishEndpointMock = CreatePublishEndpointMock(out _);
        var handler = CreateIngestHandler(publishEndpointMock.Object);
        var command = CreateIngestCommand(sensor.DeviceId);
        command.ApiKey = "wrong-key";

        var act = () => handler.Handle(command, CancellationToken.None);

        var error = await act.Should().ThrowAsync<AppError>();
        error.Which.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        publishEndpointMock.Verify(
            endpoint => endpoint.Publish(It.IsAny<SensorTelemetryReceivedMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Ingest_Should_NotPublishMessage_WhenSensorDoesNotExist()
    {
        var publishEndpointMock = CreatePublishEndpointMock(out _);
        var handler = CreateIngestHandler(publishEndpointMock.Object);

        var act = () => handler.Handle(CreateIngestCommand("missing-device"), CancellationToken.None);

        var error = await act.Should().ThrowAsync<AppError>();
        error.Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        publishEndpointMock.Verify(
            endpoint => endpoint.Publish(It.IsAny<SensorTelemetryReceivedMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Ingest_Should_NotPublishMessage_WhenSensorIsInactive()
    {
        var sensor = await CreateSensorAsync(isActive: false);
        var publishEndpointMock = CreatePublishEndpointMock(out _);
        var handler = CreateIngestHandler(publishEndpointMock.Object);

        var act = () => handler.Handle(CreateIngestCommand(sensor.DeviceId), CancellationToken.None);

        var error = await act.Should().ThrowAsync<AppError>();
        error.Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        publishEndpointMock.Verify(
            endpoint => endpoint.Publish(It.IsAny<SensorTelemetryReceivedMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Ingest_Should_ReturnDuplicateAndNotPublish_WhenMessageWasAlreadyProcessed()
    {
        var sensor = await CreateSensorAsync();
        await CreateProcessHandler().Handle(CreateProcessCommand(sensor), CancellationToken.None);
        var publishEndpointMock = CreatePublishEndpointMock(out _);
        var handler = CreateIngestHandler(publishEndpointMock.Object);

        var response = await handler.Handle(CreateIngestCommand(sensor.DeviceId), CancellationToken.None);

        response.Status.Should().Be(IngestSensorReadingStatus.Duplicate);
        publishEndpointMock.Verify(
            endpoint => endpoint.Publish(It.IsAny<SensorTelemetryReceivedMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Ingest_Should_NotExposeApiKey_InMessageOrResponse()
    {
        var sensor = await CreateSensorAsync();
        var publishEndpointMock = CreatePublishEndpointMock(out var queuedMessage);
        var handler = CreateIngestHandler(publishEndpointMock.Object);

        var response = await handler.Handle(CreateIngestCommand(sensor.DeviceId), CancellationToken.None);

        response.GetType().GetProperty("ApiKey").Should().BeNull();
        queuedMessage.Value.Should().NotBeNull();
        queuedMessage.Value!.GetType().GetProperty("ApiKey").Should().BeNull();
        typeof(ProcessSensorReadingCommand).GetProperty("ApiKey").Should().BeNull();
    }

    [Fact]
    public async Task Ingest_Should_UseCurrentUtcTimestamp_WhenTimestampIsMissing()
    {
        var sensor = await CreateSensorAsync();
        var publishEndpointMock = CreatePublishEndpointMock(out var queuedMessage);
        var handler = CreateIngestHandler(publishEndpointMock.Object);
        var command = CreateIngestCommand(sensor.DeviceId);
        command.Timestamp = null;
        var before = DateTime.UtcNow;

        await handler.Handle(command, CancellationToken.None);

        var after = DateTime.UtcNow;
        queuedMessage.Value.Should().NotBeNull();
        queuedMessage.Value!.Timestamp.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public async Task Process_Should_CreateReading_WhenMessageIsValid()
    {
        var sensor = await CreateSensorAsync();
        var handler = CreateProcessHandler();

        var response = await handler.Handle(CreateProcessCommand(sensor), CancellationToken.None);

        response.Status.Should().Be(ProcessSensorReadingStatus.Processed);
        response.SensorId.Should().Be(sensor.Id);
        response.ReadingId.Should().NotBeEmpty();
        _context.SensorReadings.Should().ContainSingle(r => r.SensorId == sensor.Id && r.MessageId == "message-1");
    }

    [Fact]
    public async Task Process_Should_UseCurrentUtcTimestamp_WhenTimestampIsDefault()
    {
        var sensor = await CreateSensorAsync();
        var handler = CreateProcessHandler();
        var command = CreateProcessCommand(sensor);
        command.Timestamp = default;
        var before = DateTime.UtcNow;

        await handler.Handle(command, CancellationToken.None);

        var after = DateTime.UtcNow;
        var reading = _context.SensorReadings.Single();
        reading.Timestamp.Should().NotBe(default);
        reading.Timestamp.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public async Task Process_Should_UpdateLastCommunication_WhenMessageIsValid()
    {
        var sensor = await CreateSensorAsync(lastCommunication: DateTime.UtcNow.AddHours(-2));
        var handler = CreateProcessHandler();

        await handler.Handle(CreateProcessCommand(sensor), CancellationToken.None);

        sensor.LastCommunication.Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task Process_Should_UpdateBatteryLevel_WhenBatteryLevelIsProvided()
    {
        var sensor = await CreateSensorAsync();
        var handler = CreateProcessHandler();

        await handler.Handle(CreateProcessCommand(sensor, batteryLevel: 87), CancellationToken.None);

        sensor.BatteryLevel.Should().Be(87);
    }

    [Fact]
    public async Task Process_Should_CreateAlert_WhenValueIsAboveMaxThreshold()
    {
        var sensor = await CreateSensorAsync(maxThreshold: 30);
        var handler = CreateProcessHandler();

        var response = await handler.Handle(CreateProcessCommand(sensor, value: 31), CancellationToken.None);

        response.AlertCreated.Should().BeTrue();
        _context.SensorAlerts.Should().ContainSingle(a => a.SensorId == sensor.Id && a.Type == AlertType.ThresholdExceeded);
    }

    [Fact]
    public async Task Process_Should_CreateAlert_WhenValueIsBelowMinThreshold()
    {
        var sensor = await CreateSensorAsync(minThreshold: 10);
        var handler = CreateProcessHandler();

        var response = await handler.Handle(CreateProcessCommand(sensor, value: 9), CancellationToken.None);

        response.AlertCreated.Should().BeTrue();
        _context.SensorAlerts.Should().ContainSingle(a => a.SensorId == sensor.Id && a.Type == AlertType.ThresholdBelowMinimum);
    }

    [Fact]
    public async Task Process_Should_NotCreateAlert_WhenValueIsInsideThreshold()
    {
        var sensor = await CreateSensorAsync(minThreshold: 10, maxThreshold: 30);
        var handler = CreateProcessHandler();

        var response = await handler.Handle(CreateProcessCommand(sensor, value: 20), CancellationToken.None);

        response.AlertCreated.Should().BeFalse();
        _context.SensorAlerts.Should().BeEmpty();
    }

    [Fact]
    public async Task Process_Should_NotDuplicateReading_WithSameMessageId()
    {
        var sensor = await CreateSensorAsync();
        var handler = CreateProcessHandler();
        await handler.Handle(CreateProcessCommand(sensor), CancellationToken.None);

        var response = await handler.Handle(CreateProcessCommand(sensor), CancellationToken.None);

        response.Status.Should().Be(ProcessSensorReadingStatus.Duplicate);
        response.AlertCreated.Should().BeFalse();
        _context.SensorReadings.Should().ContainSingle(r => r.SensorId == sensor.Id && r.MessageId == "message-1");
    }

    [Fact]
    public async Task Process_Should_NotCreateDuplicatePendingAlert_ForSameSensorAndType()
    {
        var sensor = await CreateSensorAsync(maxThreshold: 30);
        var handler = CreateProcessHandler();
        await handler.Handle(CreateProcessCommand(sensor, messageId: "message-1", value: 31), CancellationToken.None);

        var response = await handler.Handle(CreateProcessCommand(sensor, messageId: "message-2", value: 32), CancellationToken.None);

        response.AlertCreated.Should().BeFalse();
        _context.SensorAlerts.Should().ContainSingle(a => a.SensorId == sensor.Id && a.Type == AlertType.ThresholdExceeded);
        _context.SensorReadings.Count(r => r.SensorId == sensor.Id).Should().Be(2);
    }

    [Fact]
    public async Task Process_Should_SaveRawData()
    {
        var sensor = await CreateSensorAsync();
        var handler = CreateProcessHandler();
        const string rawData = "{\"firmware\":\"1.0.3\"}";

        var response = await handler.Handle(CreateProcessCommand(sensor, rawData: rawData), CancellationToken.None);

        response.Should().NotBeNull();
        _context.SensorReadings.Single().RawData.Should().Be(rawData);
    }

    private IngestSensorReadingCommandHandler CreateIngestHandler(IPublishEndpoint publishEndpoint)
    {
        return new IngestSensorReadingCommandHandler(
            _context,
            _resourceMock.Object,
            publishEndpoint,
            NullLogger<IngestSensorReadingCommandHandler>.Instance);
    }

    private ProcessSensorReadingCommandHandler CreateProcessHandler()
    {
        return new ProcessSensorReadingCommandHandler(
            _context,
            _resourceMock.Object,
            NullLogger<ProcessSensorReadingCommandHandler>.Instance);
    }

    private static Mock<IPublishEndpoint> CreatePublishEndpointMock(out CapturedMessage capturedMessage)
    {
        var capture = new CapturedMessage();
        var publishEndpointMock = new Mock<IPublishEndpoint>();
        publishEndpointMock
            .Setup(endpoint => endpoint.Publish(It.IsAny<SensorTelemetryReceivedMessage>(), It.IsAny<CancellationToken>()))
            .Callback<SensorTelemetryReceivedMessage, CancellationToken>((message, _) => capture.Value = message)
            .Returns(Task.CompletedTask);

        capturedMessage = capture;
        return publishEndpointMock;
    }

    private static IngestSensorReadingCommand CreateIngestCommand(
        string deviceId,
        string messageId = "message-1",
        double value = 25,
        double? batteryLevel = null,
        string? rawData = null)
    {
        return new IngestSensorReadingCommand
        {
            ApiKey = ApiKey,
            DeviceId = deviceId,
            MessageId = messageId,
            Timestamp = DateTime.UtcNow.AddMinutes(-5),
            Value = value,
            Unit = "C",
            BatteryLevel = batteryLevel,
            RawData = rawData,
            CorrelationId = "test-correlation"
        };
    }

    private static ProcessSensorReadingCommand CreateProcessCommand(
        Sensor sensor,
        string messageId = "message-1",
        double value = 25,
        double? batteryLevel = null,
        string? rawData = null)
    {
        return new ProcessSensorReadingCommand
        {
            SensorId = sensor.Id,
            DeviceId = sensor.DeviceId,
            MessageId = messageId,
            Timestamp = DateTime.UtcNow.AddMinutes(-5),
            Value = value,
            Unit = "C",
            BatteryLevel = batteryLevel,
            RawData = rawData,
            CorrelationId = "test-correlation"
        };
    }

    private async Task<Sensor> CreateSensorAsync(
        bool isActive = true,
        double? minThreshold = null,
        double? maxThreshold = null,
        DateTime? lastCommunication = null)
    {
        var establishment = await CreateEstablishment();
        var location = new Location
        {
            EstablishmentId = establishment.Id,
            Name = "Living room"
        };

        var sensor = new Sensor
        {
            EstablishmentId = establishment.Id,
            Location = location,
            Name = "Temperature",
            DeviceId = $"TEMP-SALA-{Guid.NewGuid():N}",
            Model = "ESP32",
            Type = SensorType.Temperature,
            ApiKey = ApiKey,
            IsActive = isActive,
            MinThreshold = minThreshold,
            MaxThreshold = maxThreshold,
            LastCommunication = lastCommunication ?? DateTime.UtcNow.AddMinutes(-10)
        };

        _context.Sensors.Add(sensor);
        await _context.SaveChangesAsync();

        return sensor;
    }

    private sealed class CapturedMessage
    {
        public SensorTelemetryReceivedMessage? Value { get; set; }
    }
}
