using HomeControllerHUB.Api.Consumers;
using HomeControllerHUB.Api.Messaging;
using HomeControllerHUB.Api.Settings;
using HomeControllerHUB.Domain.Interfaces;
using MassTransit;

namespace HomeControllerHUB.Api.Extensions;

public static class MessageBusExtensions
{
    public static IServiceCollection AddHomeControllerHubMessageBus(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));
        services.AddScoped<ISensorTelemetryQueue, MassTransitSensorTelemetryQueue>();

        services.AddMassTransit(bus =>
        {
            bus.AddConsumer<SensorTelemetryReceivedConsumer>();

            if (environment.IsEnvironment("Testing"))
            {
                bus.UsingInMemory((context, cfg) =>
                {
                    cfg.ReceiveEndpoint("sensor-telemetry-received-test", endpoint =>
                    {
                        endpoint.UseMessageRetry(retry => retry.Interval(3, TimeSpan.FromSeconds(1)));
                        endpoint.ConfigureConsumer<SensorTelemetryReceivedConsumer>(context);
                    });
                });

                return;
            }

            var settings = configuration
                .GetSection(RabbitMqSettings.SectionName)
                .Get<RabbitMqSettings>() ?? new RabbitMqSettings();

            bus.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(BuildRabbitMqUri(settings), host =>
                {
                    host.Username(settings.Username);
                    host.Password(settings.Password);
                });

                cfg.ReceiveEndpoint(settings.SensorTelemetryQueueName, endpoint =>
                {
                    endpoint.UseMessageRetry(retry => retry.Interval(3, TimeSpan.FromSeconds(2)));
                    endpoint.ConfigureConsumer<SensorTelemetryReceivedConsumer>(context);
                });
            });
        });

        return services;
    }

    private static Uri BuildRabbitMqUri(RabbitMqSettings settings)
    {
        var virtualHost = string.IsNullOrWhiteSpace(settings.VirtualHost) || settings.VirtualHost == "/"
            ? string.Empty
            : Uri.EscapeDataString(settings.VirtualHost.Trim('/'));

        return new Uri($"rabbitmq://{settings.Host}:{settings.Port}/{virtualHost}");
    }
}
