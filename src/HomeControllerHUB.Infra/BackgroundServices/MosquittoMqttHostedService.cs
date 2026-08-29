using HomeControllerHUB.Infra.Mosquitto;
using HomeControllerHUB.Infra.Mosquitto.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using System.Text;

namespace HomeControllerHUB.Infra.HostedServices;

public sealed class MosquittoMqttHostedService : BackgroundService
{
    private readonly ILogger<MosquittoMqttHostedService> _logger;
    private readonly MqttClientFactory _factory = new();
    private IMqttClient? _mqttClient;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public MosquittoMqttHostedService(ILogger<MosquittoMqttHostedService> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _mqttClient = _factory.CreateMqttClient();

        _mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            var topic = e.ApplicationMessage.Topic;

            #region código legal
            // Vou deixar comentado, pois esse código abaixo é muito bonito, e quero olhar mais vezes para ele.
            //Type consumerType = typeof(IBrokerConsumer);

            //IEnumerable<Type> consumers = Assembly
            //.GetExecutingAssembly()
            //.GetTypes()
            //.Where(type =>
            //    consumerType.IsAssignableFrom(type)
            //    && type.IsClass
            //    && !type.IsAbstract);

            //foreach(var consumer in consumers)
            //{
            //    var instance = Activator.CreateInstance(consumer) as IBrokerConsumer;
            //    if (instance is not null && instance.Topic == e.ApplicationMessage.Topic)
            //    {
            //        _ = instance.ExecuteTopicAsync(payload);
            //    }
            //}
            #endregion

            using var scope = _serviceScopeFactory.CreateScope();

            var dispatcher = scope.ServiceProvider.GetRequiredService<MosquittoDispatcher>();

            await dispatcher.DispatchAsync(topic, payload, stoppingToken);
        };

        _mqttClient.DisconnectedAsync += async e =>
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            _logger.LogWarning("MQTT disconnected. Trying to reconnect in 5 seconds.");

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            try
            {
                await ConnectAndSubscribeAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reconnect to MQTT broker.");
            }
        };

        await ConnectAndSubscribeAsync(stoppingToken);

        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }

    private async Task ConnectAndSubscribeAsync(CancellationToken cancellationToken)
    {
        if (_mqttClient is null)
        {
            return;
        }

        if (!_mqttClient.IsConnected)
        {
            var options = new MqttClientOptionsBuilder()
                .WithClientId("homecontrollerhub-api")
                .WithTcpServer("localhost", 1883)
                .WithCleanSession(false)
                .Build();

            await _mqttClient.ConnectAsync(options, cancellationToken);

            _logger.LogInformation("Connected to MQTT broker.");
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var topics = scope.ServiceProvider.GetRequiredService<IEnumerable<IBrokerConsumer>>();


        var subscribeOptionsBuilder = _factory.CreateSubscribeOptionsBuilder();

        foreach (var consumer in topics)
        {
            subscribeOptionsBuilder.WithTopicFilter(f =>
            {
                f.WithTopic(consumer.Topic);
            });
        }

        var subscribeOptions = subscribeOptionsBuilder.Build();

        await _mqttClient.SubscribeAsync(subscribeOptions, cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_mqttClient?.IsConnected == true)
        {
            await _mqttClient.DisconnectAsync(cancellationToken: cancellationToken);
        }

        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _mqttClient?.Dispose();
        base.Dispose();
    }
}