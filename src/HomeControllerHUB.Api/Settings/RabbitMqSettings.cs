namespace HomeControllerHUB.Api.Settings;

public class RabbitMqSettings
{
    public const string SectionName = "RabbitMq";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string SensorTelemetryQueueName { get; set; } = "sensor-telemetry-received";
    public int PublishTimeoutSeconds { get; set; } = 5;
}
