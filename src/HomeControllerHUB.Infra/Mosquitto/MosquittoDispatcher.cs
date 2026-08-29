using HomeControllerHUB.Infra.HostedServices;
using HomeControllerHUB.Infra.Mosquitto.Interfaces;
using Microsoft.Extensions.Logging;

namespace HomeControllerHUB.Infra.Mosquitto;

public class MosquittoDispatcher
{
    private readonly IEnumerable<IBrokerConsumer> _consumers;
    private readonly ILogger<MosquittoDispatcher> _logger;


    public MosquittoDispatcher(IEnumerable<IBrokerConsumer> consumers, ILogger<MosquittoDispatcher> logger)
    {
        _consumers = consumers;
        _logger = logger;
    }

    public async Task DispatchAsync(
       string topic,
       string payload,
       CancellationToken cancellationToken)
    {
        IBrokerConsumer? consumer = _consumers.FirstOrDefault(
            consumer => consumer.Topic == topic);

        if (consumer is null)
        {
            _logger.LogInformation($"Não foi encontrado nenhum tópico configurado: {topic}");
            return;
        }

        await consumer.ExecuteTopicAsync(payload, cancellationToken);
    }
}
