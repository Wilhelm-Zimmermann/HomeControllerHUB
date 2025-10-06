using FluentAssertions;
using FluentValidation.TestHelper;
using HomeControllerHUB.Application.Locations.Commands.DeleteLocation;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using Moq;

namespace HomeControllerHUB.Application.Tests.Locations.Commands;

public class DeleteLocationCommandTest : TestConfigs
{
    private readonly Mock<ISharedResource> _resourceMock;

    public DeleteLocationCommandTest()
    {
        _resourceMock = new Mock<ISharedResource>();
    }

    [Fact]
    public async Task Delete_Should_Succeed_WhenLocationHasNoDependencies()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var location = new Location
        {
            EstablishmentId = establishment.Id,
            Name = "Deletable Location"
        };
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        var command = new DeleteLocationCommand { Id = location.Id };
        var handler = new DeleteLocationCommandHandler(_context, _resourceMock.Object);

        // ACT
        await handler.Handle(command, CancellationToken.None);

        // ASSERT
        var locationInDb = await _context.Locations.FindAsync(location.Id);
        locationInDb.Should().BeNull();
    }

    [Fact]
    public async Task Delete_Should_ThrowAppError_WhenLocationNotFound()
    {
        // ARRANGE
        var command = new DeleteLocationCommand { Id = Guid.NewGuid() };
        var handler = new DeleteLocationCommandHandler(_context, _resourceMock.Object);
        _resourceMock.Setup(r => r.NotFoundMessage(nameof(Location))).Returns("Location not found.");

        // ACT
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // ASSERT
        await act.Should()
            .ThrowAsync<AppError>()
            .Where(ex => ex.StatusCode == 404);
    }

    [Fact]
    public async Task Delete_Should_ThrowAppError_WhenLocationHasChildLocations()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var parentLocation = new Location { EstablishmentId = establishment.Id, Name = "Parent" };
        var childLocation = new Location
            { EstablishmentId = establishment.Id, Name = "Child", ParentLocationId = parentLocation.Id };
        _context.Locations.AddRange(parentLocation, childLocation);
        await _context.SaveChangesAsync();

        var command = new DeleteLocationCommand { Id = parentLocation.Id };
        var handler = new DeleteLocationCommandHandler(_context, _resourceMock.Object);
        _resourceMock.Setup(r => r.Message("DeleteAllChildLocationsFirst"))
            .Returns("Please delete all child locations first.");

        // ACT
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // ASSERT
        await act.Should()
            .ThrowAsync<AppError>()
            .Where(ex => ex.StatusCode == 400);
    }

    [Fact]
    public async Task Delete_Should_ThrowAppError_WhenLocationHasSensors()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var location = new Location { EstablishmentId = establishment.Id, Name = "Location With Sensor" };
        _context.Locations.Add(location);

        var sensor = new Sensor
        {
            Name = "Temperature Sensor", 
            LocationId = location.Id, 
            EstablishmentId = establishment.Id,
            Type = SensorType.Temperature,
            DeviceId = "temp-akg-211",
            Model = "arduino nano"
        };
        _context.Sensors.Add(sensor);
        await _context.SaveChangesAsync();

        var command = new DeleteLocationCommand { Id = location.Id };
        var handler = new DeleteLocationCommandHandler(_context, _resourceMock.Object);
        _resourceMock.Setup(r => r.Message("MoveSensorsBeforeDeleting"))
            .Returns("Please move or delete the sensors associated with this location before deleting.");

        // ACT
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // ASSERT
        await act.Should()
            .ThrowAsync<AppError>()
            .Where(ex => ex.StatusCode == 400);
    }

    [Fact]
    public async Task Validation_Should_Fail_WhenIdIsEmpty()
    {
        // ARRANGE
        var validator = new DeleteLocationCommandValidator(_context);
        var command = new DeleteLocationCommand { Id = Guid.Empty };

        // ACT
        var result = await validator.TestValidateAsync(command);

        // ASSERT
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public async Task Validation_Should_Fail_WhenLocationDoesNotExist()
    {
        // ARRANGE
        var validator = new DeleteLocationCommandValidator(_context);
        var command = new DeleteLocationCommand { Id = Guid.NewGuid() };

        // ACT
        var result = await validator.TestValidateAsync(command);

        // ASSERT
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("The specified location does not exist.");
    }

    [Fact]
    public async Task Validation_Should_Succeed_WhenLocationExists()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var location = new Location { EstablishmentId = establishment.Id, Name = "Existing Location" };
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        var validator = new DeleteLocationCommandValidator(_context);
        var command = new DeleteLocationCommand { Id = location.Id };

        // ACT
        var result = await validator.TestValidateAsync(command);

        // ASSERT
        result.ShouldNotHaveAnyValidationErrors();
    }
}