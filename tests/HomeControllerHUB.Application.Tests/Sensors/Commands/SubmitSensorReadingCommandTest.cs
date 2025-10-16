using FluentAssertions;
using HomeControllerHUB.Application.Sensors.Commands.SubmitSensorReading;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Globalization;
using Moq;

namespace HomeControllerHUB.Application.Tests.Sensors;

public class SubmitSensorReadingCommandTest : TestConfigs
{
    private readonly Mock<ISharedResource> _resourceMock;

    public SubmitSensorReadingCommandTest()
    {
        _resourceMock = new Mock<ISharedResource>();
    }

    [Fact]
    public async Task Submit_Should_CreateReadingAndAlert_WhenValueExceedsThreshold()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        _resourceMock.Setup(r => r.Message(It.IsAny<string>())).Returns("Error message");
        var location = new Location { EstablishmentId = establishment.Id, Name = "L" };
        var sensor = new Sensor 
        { 
            Name = "N", DeviceId = "D", Model = "M", Type = SensorType.Door, IsActive = true,
            EstablishmentId = establishment.Id, Location = location, MaxThreshold = 50 
        };
        _context.Sensors.Add(sensor);
        await _context.SaveChangesAsync();

        var command = new SubmitSensorReadingCommand { DeviceId = sensor.DeviceId, Value = 55 };
        var handler = new SubmitSensorReadingCommandHandler(_context, _resourceMock.Object);

        // ACT
        await handler.Handle(command, CancellationToken.None);

        // ASSERT
        var readingInDb = _context.SensorReadings.FirstOrDefault(r => r.SensorId == sensor.Id);
        readingInDb.Should().NotBeNull();
        readingInDb!.Value.Should().Be(55);

        var alertInDb = _context.SensorAlerts.FirstOrDefault(a => a.SensorId == sensor.Id);
        alertInDb.Should().NotBeNull();
        alertInDb!.Type.Should().Be(AlertType.ThresholdExceeded);
    }

    [Fact]
    public async Task Submit_Should_BeIgnored_WhenSensorIsInactive()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var location = new Location { EstablishmentId = establishment.Id, Name = "L" };
        var sensor = new Sensor 
        { 
            Name = "N", DeviceId = "D", Model = "M", Type = SensorType.Door, IsActive = false,
            EstablishmentId = establishment.Id, Location = location
        };
        _context.Sensors.Add(sensor);
        await _context.SaveChangesAsync();

        var command = new SubmitSensorReadingCommand { DeviceId = sensor.DeviceId, Value = 25 };
        var handler = new SubmitSensorReadingCommandHandler(_context, _resourceMock.Object);

        // ACT
        await handler.Handle(command, CancellationToken.None);

        // ASSERT
        _context.SensorReadings.Count().Should().Be(0);
        _context.SensorAlerts.Count().Should().Be(0);
    }
}
