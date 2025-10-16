using FluentAssertions;
using HomeControllerHUB.Application.Sensors.Commands.SubmitSensorReadingBatch;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Globalization;
using Moq;

namespace HomeControllerHUB.Application.Tests.Sensors;

public class SubmitSensorReadingBatchCommandTest : TestConfigs
{
    private readonly Mock<ISharedResource> _resourceMock;

    public SubmitSensorReadingBatchCommandTest()
    {
        _resourceMock = new Mock<ISharedResource>();
    }

    [Fact]
    public async Task SubmitBatch_Should_CreateMultipleReadingsAndAlerts()
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

        var command = new SubmitSensorReadingBatchCommand
        {
            DeviceId = sensor.DeviceId,
            Readings = new List<SensorReadingDto>
            {
                new() { Value = 45 },
                new() { Value = 55 },
                new() { Value = 65 } 
            }
        };
        var handler = new SubmitSensorReadingBatchCommandHandler(_context, _resourceMock.Object);
        
        // ACT
        await handler.Handle(command, CancellationToken.None);

        // ASSERT
        _context.SensorReadings.Count(r => r.SensorId == sensor.Id).Should().Be(3);
        _context.SensorAlerts.Count(a => a.SensorId == sensor.Id).Should().Be(2);
    }
}
