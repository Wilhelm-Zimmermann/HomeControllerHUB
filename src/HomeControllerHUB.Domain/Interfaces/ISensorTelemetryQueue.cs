using HomeControllerHUB.Domain.Messages;

namespace HomeControllerHUB.Domain.Interfaces;

public interface ISensorTelemetryQueue
{
    Task EnqueueAsync(SensorTelemetryReceivedMessage message, CancellationToken cancellationToken);
}
