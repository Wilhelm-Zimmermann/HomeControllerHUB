using HomeControllerHUB.Domain.Messages;
using HomeControllerHUB.Infra.Mosquitto.Interfaces;
using MassTransit;

namespace HomeControllerHUB.Infra.Mosquitto;

public class SensorTelemetryReceivedConsumer : IBrokerConsumer
{
    public string Topic => "sensor/telemetry";
    private readonly IPublishEndpoint _publishEndpoint;
    
    public SensorTelemetryReceivedConsumer(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task ExecuteTopicAsync(string payload, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Received telemetry for sensor: {payload}");
        var message = new SensorTelemetryReceivedMessage
        {
            DeviceId = "123"
        };
        
        await _publishEndpoint.Publish(message, cancellationToken);
    }
}
