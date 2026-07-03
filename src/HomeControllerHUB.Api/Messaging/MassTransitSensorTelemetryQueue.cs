using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Messages;
using HomeControllerHUB.Api.Settings;
using MassTransit;
using Microsoft.Extensions.Options;

namespace HomeControllerHUB.Api.Messaging;

public class MassTransitSensorTelemetryQueue : ISensorTelemetryQueue
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly RabbitMqSettings _settings;

    public MassTransitSensorTelemetryQueue(
        IPublishEndpoint publishEndpoint,
        IOptions<RabbitMqSettings> settings)
    {
        _publishEndpoint = publishEndpoint;
        _settings = settings.Value;
    }

    public async Task EnqueueAsync(SensorTelemetryReceivedMessage message, CancellationToken cancellationToken)
    {
        using var publishTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        publishTimeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(_settings.PublishTimeoutSeconds, 1)));

        await _publishEndpoint.Publish(message, publishTimeout.Token);
    }
}
