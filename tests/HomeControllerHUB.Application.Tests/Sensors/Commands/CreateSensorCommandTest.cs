using FluentAssertions;
using FluentValidation.TestHelper;
using HomeControllerHUB.Application.Sensors.Commands.CreateSensor;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using Moq;

namespace HomeControllerHUB.Application.Tests.Sensors;

public class CreateSensorCommandTest : TestConfigs
{
    private readonly Mock<ISharedResource> _resourceMock;

    public CreateSensorCommandTest()
    {
        _resourceMock = new Mock<ISharedResource>();
    }

    [Fact]
    public async Task Create_Should_Succeed_WithValidData()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var location = new Location { Name = "Living Room", EstablishmentId = establishment.Id };
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        var command = new CreateSensorCommand
        {
            EstablishmentId = establishment.Id,
            LocationId = location.Id,
            Name = "Temperature Sensor",
            DeviceId = "TEMP-LR-01",
            Type = SensorType.Temperature,
            Model = "DHT22"
        };
        var handler = new CreateSensorCommandHandler(_context, _resourceMock.Object);

        // ACT
        var sensorId = await handler.Handle(command, CancellationToken.None);

        // ASSERT
        sensorId.Should().NotBe(Guid.Empty);
        var sensorInDb = await _context.Sensors.FindAsync(sensorId);
        sensorInDb.Should().NotBeNull();
        sensorInDb!.Name.Should().Be(command.Name);
        sensorInDb.ApiKey.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Create_Should_ThrowAppError_WhenDeviceIdIsInUse()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var location = new Location { Name = "Garage", EstablishmentId = establishment.Id };
        var existingSensor = new Sensor { DeviceId = "UNIQUE-01", EstablishmentId = establishment.Id, Location = location, Model="M", Name="N", Type=SensorType.Humidity };
        _context.Sensors.Add(existingSensor);
        await _context.SaveChangesAsync();
        
        var command = new CreateSensorCommand { DeviceId = "UNIQUE-01", EstablishmentId = establishment.Id, LocationId = location.Id, Name = "N", Model = "M" };
        var handler = new CreateSensorCommandHandler(_context, _resourceMock.Object);

        // ACT
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // ASSERT
        await act.Should().ThrowAsync<AppError>().Where(e => e.StatusCode == 400);
    }

    [Fact]
    public async Task Validation_Should_Fail_WhenMaxThresholdIsLessThanMin()
    {
        // ARRANGE
        var validator = new CreateSensorCommandValidator(_context);
        var command = new CreateSensorCommand
        {
            MinThreshold = 25,
            MaxThreshold = 20
        };

        // ACT
        var result = await validator.TestValidateAsync(command);
        
        // ASSERT
        result.ShouldHaveValidationErrorFor(x => x.MaxThreshold)
            .WithErrorMessage("Maximum threshold must be greater than minimum threshold.");
    }
}
