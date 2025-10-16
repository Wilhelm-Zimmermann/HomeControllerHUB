using FluentAssertions;
using HomeControllerHUB.Application.Sensors.Commands.DeleteSensor;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using Moq;

namespace HomeControllerHUB.Application.Tests.Sensors;

public class DeleteSensorCommandTest : TestConfigs
{
    private readonly Mock<ISharedResource> _resourceMock;

    public DeleteSensorCommandTest()
    {
        _resourceMock = new Mock<ISharedResource>();
    }

    [Fact]
    public async Task Delete_Should_Succeed_WhenSensorHasNoDependencies()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var location = new Location { EstablishmentId = establishment.Id, Name = "L" };
        var sensor = new Sensor { Name = "N", DeviceId = "D", Model = "M", Type = SensorType.Door, EstablishmentId = establishment.Id, Location = location };
        _context.Sensors.Add(sensor);
        await _context.SaveChangesAsync();
        
        var command = new DeleteSensorCommand { Id = sensor.Id };
        var handler = new DeleteSensorCommandHandler(_context, _resourceMock.Object);

        // ACT
        await handler.Handle(command, CancellationToken.None);

        // ASSERT
        var sensorInDb = await _context.Sensors.FindAsync(sensor.Id);
        sensorInDb.Should().BeNull();
    }

    [Fact]
    public async Task Delete_Should_ThrowAppError_WhenSensorHasReadings()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var location = new Location { EstablishmentId = establishment.Id, Name = "L" };
        var sensor = new Sensor { Name = "N", DeviceId = "D", Model = "M", Type = SensorType.Door, EstablishmentId = establishment.Id, Location = location };
        _context.Sensors.Add(sensor);
        _context.SensorReadings.Add(new SensorReading { Sensor = sensor, Value = 10 });
        await _context.SaveChangesAsync();
        
        var command = new DeleteSensorCommand { Id = sensor.Id };
        var handler = new DeleteSensorCommandHandler(_context, _resourceMock.Object);

        // ACT
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // ASSERT
        await act.Should().ThrowAsync<AppError>().Where(e => e.StatusCode == 400);
    }
}
