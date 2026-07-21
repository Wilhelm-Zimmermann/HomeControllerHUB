using HomeControllerHUB.Infra.Mosquitto.Interfaces;

namespace HomeControllerHUB.Infra.Mosquitto;

public class SensorTelemetryReceivedMessage : IBrokerConsumer
{
    public string Topic => "sensor/telemetry";

    public Task ExecuteTopicAsync(string payload)
    {
        Console.WriteLine($"Received telemetry for sensor: {payload}");
        return Task.CompletedTask;
    }
}
