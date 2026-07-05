namespace HomeControllerHUB.Api.Settings;

public class RabbitMqSettings
{
    public const string SectionName = "RabbitMq";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string QueueName { get; set; } = "homecontrollerhub.sensor-telemetry";
    public int PublishTimeoutSeconds { get; set; } = 5;

    public string SensorTelemetryQueueName
    {
        get => QueueName;
        set => QueueName = value;
    }
}
