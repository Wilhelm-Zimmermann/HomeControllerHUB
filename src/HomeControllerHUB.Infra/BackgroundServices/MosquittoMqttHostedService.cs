using HomeControllerHUB.Infra.Mosquitto;
using HomeControllerHUB.Infra.Mosquitto.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Writers;
using MQTTnet;
using System.Reflection;
using System.Text;

namespace HomeControllerHUB.Api.HostedServices;

public sealed class MosquittoMqttHostedService : BackgroundService
{
    private readonly ILogger<MosquittoMqttHostedService> _logger;
    private readonly MqttClientFactory _factory = new();
    private IMqttClient? _mqttClient;
    private MosquittoDispatcher _dispatcher;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public MosquittoMqttHostedService(ILogger<MosquittoMqttHostedService> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _mqttClient = _factory.CreateMqttClient();
        using var scope = _serviceScopeFactory.CreateScope();
        _dispatcher = scope.ServiceProvider.GetRequiredService<MosquittoDispatcher>();

        _mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            _logger.LogInformation(
                "Received MQTT message. Topic: {Topic}. Payload: {Payload}",
                e.ApplicationMessage.Topic,
                payload);
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

            await _dispatcher.DispatchAsync(e.ApplicationMessage.Topic, payload);
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

        var subscribeOptions = _factory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(f =>
            {
                foreach(var topic in topics)
                {
                    f.WithTopic(topic.Topic);
                }
            })
            .Build();

        await _mqttClient.SubscribeAsync(subscribeOptions, cancellationToken);

        _logger.LogInformation("Subscribed to MQTT topic my/test/topic.");
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