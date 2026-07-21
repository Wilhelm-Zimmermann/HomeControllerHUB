using HomeControllerHUB.Api.Consumers;
using MassTransit;

namespace HomeControllerHUB.Api.IntegrationTests;

public class MessageBusConfigurationTests
{
    [Fact]
    public void Kebab_case_formatter_generates_expected_consumer_endpoint_name()
    {
        var formatter = new KebabCaseEndpointNameFormatter(includeNamespace: false);

        var endpointName = formatter.Consumer<SensorTelemetryReceivedConsumer>();

        Assert.Equal("sensor-telemetry-received", endpointName);
    }
}
