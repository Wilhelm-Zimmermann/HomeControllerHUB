using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Messages;
using HomeControllerHUB.Api.Settings;
using MassTransit;
using Microsoft.Extensions.Options;

namespace HomeControllerHUB.Api.Messaging;

public class MassTransitSensorTelemetryQueue : ISensorTelemetryQueue
{
    private readonly ISendEndpointProvider _sendEndpointProvider;
    private readonly RabbitMqSettings _settings;

    public MassTransitSensorTelemetryQueue(
        ISendEndpointProvider sendEndpointProvider,
        IOptions<RabbitMqSettings> settings)
    {
        _sendEndpointProvider = sendEndpointProvider;
        _settings = settings.Value;
    }

    public async Task EnqueueAsync(SensorTelemetryReceivedMessage message, CancellationToken cancellationToken)
    {
        using var publishTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        publishTimeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(_settings.PublishTimeoutSeconds, 1)));

        var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{_settings.QueueName}"));
        await endpoint.Send(message, publishTimeout.Token);
    }
}
