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
        var settings = configuration
            .GetSection(RabbitMqSettings.SectionName)
            .Get<RabbitMqSettings>() ?? new RabbitMqSettings();

        services.AddMassTransit(bus =>
        {
            bus.SetKebabCaseEndpointNameFormatter();
            bus.AddConsumers(typeof(IConsumerAssemblyMarker).Assembly);
            bus.AddConfigureEndpointsCallback((_, _, endpoint) =>
            {
                var retryDelay = environment.IsEnvironment("Testing")
                    ? TimeSpan.FromSeconds(1)
                    : TimeSpan.FromSeconds(2);

                endpoint.UseMessageRetry(retry => retry.Interval(3, retryDelay));

                if (endpoint is IRabbitMqReceiveEndpointConfigurator rabbitMqEndpoint)
                {
                    rabbitMqEndpoint.Durable = true;
                }
            });

            if (environment.IsEnvironment("Testing"))
            {
                bus.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });

                return;
            }

            bus.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(BuildRabbitMqUri(settings), host =>
                {
                    host.Username(settings.Username);
                    host.Password(settings.Password);
                });

                cfg.ConfigureEndpoints(context);
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
