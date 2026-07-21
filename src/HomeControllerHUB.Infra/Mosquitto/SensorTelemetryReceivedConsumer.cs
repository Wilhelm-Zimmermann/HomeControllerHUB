using HomeControllerHUB.Infra.Mosquitto.Interfaces;

namespace HomeControllerHUB.Infra.Mosquitto;

public class SensorTelemetryReceivedConsumer : IBrokerConsumer
{
    public string Topic => "sensor/telemetry";

    public Task ExecuteTopicAsync(string payload, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Received telemetry for sensor: {payload}");
        return Task.CompletedTask;
    }
}
