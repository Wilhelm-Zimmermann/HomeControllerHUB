using FluentAssertions;
using HomeControllerHUB.Application.Sensors.Commands.UpdateSensor;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Globalization;
using Moq;

namespace HomeControllerHUB.Application.Tests.Sensors;

public class UpdateSensorCommandTest : TestConfigs
{
    private readonly Mock<ISharedResource> _resourceMock;

    public UpdateSensorCommandTest()
    {
        _resourceMock = new Mock<ISharedResource>();
    }

    [Fact]
    public async Task Update_Should_Succeed_WithValidData()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var initialLocation = new Location { Name = "Old Location", EstablishmentId = establishment.Id };
        var newLocation = new Location { Name = "New Location", EstablishmentId = establishment.Id };
        _context.Locations.AddRange(initialLocation, newLocation);
        var sensor = new Sensor { Name = "Old Name", DeviceId = "OLD-ID", Model = "M", Type = SensorType.Temperature, IsActive = true, EstablishmentId = establishment.Id, Location = initialLocation };
        _context.Sensors.Add(sensor);
        await _context.SaveChangesAsync();

        var command = new UpdateSensorCommand
        {
            Id = sensor.Id,
            Name = "New Name",
            DeviceId = "NEW-ID",
            IsActive = false,
            LocationId = newLocation.Id,
            EstablishmentId = establishment.Id,
            Model = "New Model",
            Type = SensorType.Humidity
        };
        var handler = new UpdateSensorCommandHandler(_context, _resourceMock.Object);

        // ACT
        await handler.Handle(command, CancellationToken.None);

        // ASSERT
        var sensorInDb = await _context.Sensors.FindAsync(sensor.Id);
        sensorInDb.Should().NotBeNull();
        sensorInDb!.Name.Should().Be("New Name");
        sensorInDb.DeviceId.Should().Be("NEW-ID");
        sensorInDb.IsActive.Should().BeFalse();
        sensorInDb.LocationId.Should().Be(newLocation.Id);
    }
}
