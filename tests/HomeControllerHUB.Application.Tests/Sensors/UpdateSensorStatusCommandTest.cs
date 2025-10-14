using FluentAssertions;
using HomeControllerHUB.Application.Sensors.Commands.UpdateSensorStatus;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Globalization;
using Moq;

namespace HomeControllerHUB.Application.Tests.Sensors;

public class UpdateSensorStatusCommandTest : TestConfigs
{
    private readonly Mock<ISharedResource> _resourceMock;

    public UpdateSensorStatusCommandTest()
    {
        _resourceMock = new Mock<ISharedResource>();
    }

    [Fact]
    public async Task UpdateStatus_Should_UpdateSensorAndCreateStatusRecord()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var location = new Location { EstablishmentId = establishment.Id, Name = "L" };
        var sensor = new Sensor 
        { 
            Name = "N", DeviceId = "D", Model = "M", Type = SensorType.Door, IsActive = true,
            EstablishmentId = establishment.Id, Location = location, FirmwareVersion = "1.0"
        };
        _context.Sensors.Add(sensor);
        await _context.SaveChangesAsync();

        var command = new UpdateSensorStatusCommand
        {
            DeviceId = sensor.DeviceId,
            IsActive = false,
            FirmwareVersion = "1.1",
            BatteryLevel = 80
        };
        var handler = new UpdateSensorStatusCommandHandler(_context, _resourceMock.Object);
        
        // ACT
        await handler.Handle(command, CancellationToken.None);

        // ASSERT
        var sensorInDb = await _context.Sensors.FindAsync(sensor.Id);
        sensorInDb!.IsActive.Should().BeFalse();
        sensorInDb.FirmwareVersion.Should().Be("1.1");
        sensorInDb.LastCommunication.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        
        var statusUpdateInDb = _context.SensorStatusUpdates.FirstOrDefault(su => su.SensorId == sensor.Id);
        statusUpdateInDb.Should().NotBeNull();
        statusUpdateInDb!.BatteryLevel.Should().Be(80);
    }

    [Fact]
    public async Task UpdateStatus_Should_CreateLowBatteryAlert()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var location = new Location { EstablishmentId = establishment.Id, Name = "L" };
        var sensor = new Sensor 
        { 
            Name = "N", DeviceId = "D", Model = "M", Type = SensorType.Door, IsActive = true,
            EstablishmentId = establishment.Id, Location = location
        };
        _context.Sensors.Add(sensor);
        await _context.SaveChangesAsync();
        
        var command = new UpdateSensorStatusCommand { DeviceId = sensor.DeviceId, BatteryLevel = 14 };
        var handler = new UpdateSensorStatusCommandHandler(_context, _resourceMock.Object);

        // ACT
        await handler.Handle(command, CancellationToken.None);

        // ASSERT
        var alertInDb = _context.SensorAlerts.FirstOrDefault(a => a.SensorId == sensor.Id);
        alertInDb.Should().NotBeNull();
        alertInDb!.Type.Should().Be(AlertType.BatteryLow);
    }
}
