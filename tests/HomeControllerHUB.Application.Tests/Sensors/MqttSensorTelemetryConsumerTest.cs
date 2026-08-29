using FluentAssertions;
using HomeControllerHUB.Application.Sensors.Commands.IngestSensorReading;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MqttSensorTelemetryConsumer = HomeControllerHUB.Api.Mosquitto.SensorTelemetryReceivedConsumer;

namespace HomeControllerHUB.Application.Tests.Sensors;

public class MqttSensorTelemetryConsumerTest
{
    [Fact]
    public async Task ExecuteTopicAsync_Should_MapPayloadWithoutTimestamp_ToIngestCommand()
    {
        IngestSensorReadingCommand? capturedCommand = null;
        var senderMock = new Mock<ISender>();
        senderMock
            .Setup(sender => sender.Send(It.IsAny<IngestSensorReadingCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<IngestSensorReadingResponse>, CancellationToken>(
                (command, _) => capturedCommand = (IngestSensorReadingCommand)command)
            .ReturnsAsync(new IngestSensorReadingResponse());
        var consumer = new MqttSensorTelemetryConsumer(
            senderMock.Object,
            NullLogger<MqttSensorTelemetryConsumer>.Instance);

        await consumer.ExecuteTopicAsync("""
            {
              "apiKey": "sensor-secret",
              "deviceId": "ESP32-TEMP-1",
              "messageId": "mqtt-message-1",
              "value": 24.5,
              "unit": "C"
            }
            """, CancellationToken.None);

        capturedCommand.Should().NotBeNull();
        capturedCommand!.ApiKey.Should().Be("sensor-secret");
        capturedCommand.DeviceId.Should().Be("ESP32-TEMP-1");
        capturedCommand.MessageId.Should().Be("mqtt-message-1");
        capturedCommand.Timestamp.Should().BeNull();
        capturedCommand.Value.Should().Be(24.5);
    }

    [Fact]
    public async Task ExecuteTopicAsync_Should_RejectInvalidJson_WithoutSendingCommand()
    {
        var senderMock = new Mock<ISender>();
        var consumer = new MqttSensorTelemetryConsumer(
            senderMock.Object,
            NullLogger<MqttSensorTelemetryConsumer>.Instance);

        var act = () => consumer.ExecuteTopicAsync("{", CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        senderMock.Verify(
            sender => sender.Send(It.IsAny<IngestSensorReadingCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
