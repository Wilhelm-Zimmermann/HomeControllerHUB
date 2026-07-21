using HomeControllerHUB.Infra.Mosquitto.Interfaces;

namespace HomeControllerHUB.Infra.Mosquitto;

public class MosquittoDispatcher
{
    private readonly IEnumerable<IBrokerConsumer> _consumers;
    public MosquittoDispatcher(IEnumerable<IBrokerConsumer> consumers)
    {
        _consumers = consumers;
    }

    public async Task DispatchAsync(
       string topic,
       string payload)
    {
        IBrokerConsumer? consumer = _consumers.FirstOrDefault(
            consumer => consumer.Topic == topic);

        if (consumer is null)
        {
            throw new InvalidOperationException(
                $"Nenhum consumer foi registrado para o tópico '{topic}'.");
        }

        await consumer.ExecuteTopicAsync(payload);
    }
}
