namespace HomeControllerHUB.Infra.Mosquitto.Interfaces;

public interface IBrokerConsumer
{
    public string Topic { get; }
    public Task ExecuteTopicAsync(string payload);
}
