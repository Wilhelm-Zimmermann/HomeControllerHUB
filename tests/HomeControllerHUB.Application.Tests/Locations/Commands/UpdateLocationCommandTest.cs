using FluentAssertions;
using FluentValidation.TestHelper;
using HomeControllerHUB.Application.Locations.Commands.UpdateLocation;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using Moq;

namespace HomeControllerHUB.Application.Tests.Locations.Commands;

public class UpdateLocationCommandTest : TestConfigs
{
    private readonly Mock<ISharedResource> _resourceMock;

    public UpdateLocationCommandTest()
    {
        _resourceMock = new Mock<ISharedResource>();
    }
    
    [Fact]
    public async Task Update_Should_Succeed_WithValidParameters()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var initialLocation = new Location
        {
            EstablishmentId = establishment.Id,
            Name = "Old Name",
            Description = "Old Description",
            Type = LocationType.Area
        };
        _context.Locations.Add(initialLocation);
        await _context.SaveChangesAsync();

        var command = new UpdateLocationCommand
        {
            Id = initialLocation.Id,
            Name = "Updated Living Room",
            Description = "Updated main living area.",
            Type = LocationType.Room,
            ParentLocationId = null
        };
        
        var handler = new UpdateLocationCommandHandler(_context, _resourceMock.Object);

        // ACT
        await handler.Handle(command, CancellationToken.None);

        // ASSERT
        var locationInDb = await _context.Locations.FindAsync(initialLocation.Id);
        locationInDb.Should().NotBeNull();
        locationInDb!.Name.Should().Be(command.Name);
        locationInDb.Description.Should().Be(command.Description);
        locationInDb.Type.Should().Be(command.Type);
    }
    
    [Fact]
    public async Task Update_Should_ThrowAppError_WhenLocationNotFound()
    {
        // ARRANGE
        var command = new UpdateLocationCommand { Id = Guid.NewGuid(), Name = "test" };
        var handler = new UpdateLocationCommandHandler(_context, _resourceMock.Object);
        _resourceMock.Setup(r => r.NotFoundMessage(nameof(Location))).Returns("Location not found.");

        // ACT
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // ASSERT
        await act.Should()
            .ThrowAsync<AppError>()
            .Where(ex => ex.StatusCode == 404);
    }
    
    [Fact]
    public async Task Update_Should_ThrowAppError_WhenLocationIsItsOwnParent()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var location = new Location { EstablishmentId = establishment.Id, Name = "Some Location" };
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        var command = new UpdateLocationCommand
        {
            Id = location.Id,
            Name = "New Name",
            ParentLocationId = location.Id
        };

        var handler = new UpdateLocationCommandHandler(_context, _resourceMock.Object);
        _resourceMock.Setup(r => r.Message("LocationCannotBeItsOwnParent")).Returns("A location cannot be its own parent.");

        // ACT
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // ASSERT
        await act.Should()
            .ThrowAsync<AppError>()
            .Where(ex => ex.StatusCode == 400);
    }
    
    [Fact]
    public async Task Validation_Should_Fail_WhenLocationDoesNotExist()
    {
        // ARRANGE
        var validator = new UpdateLocationCommandValidator(_context);
        var command = new UpdateLocationCommand { Id = Guid.NewGuid(), Name = "Test" };
        
        // ACT
        var result = await validator.TestValidateAsync(command);

        // ASSERT
        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("The specified location does not exist.");
    }
    
    [Fact]
    public async Task Validation_Should_Fail_WhenNameIsEmpty()
    {
        // ARRANGE
        var validator = new UpdateLocationCommandValidator(_context);
        var command = new UpdateLocationCommand { Id = Guid.NewGuid(), Name = "" };
        
        // ACT
        var result = await validator.TestValidateAsync(command);

        // ASSERT
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
    
    [Fact]
    public async Task Validation_Should_Fail_WhenParentLocationDoesNotExist()
    {
        // ARRANGE
        var validator = new UpdateLocationCommandValidator(_context);
        var establishment = await CreateEstablishment();
        var location = new Location { EstablishmentId = establishment.Id, Name = "Existing Location" };
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        var command = new UpdateLocationCommand
        {
            Id = location.Id,
            Name = "Valid Name",
            ParentLocationId = Guid.NewGuid()
        };
        
        // ACT
        var result = await validator.TestValidateAsync(command);

        // ASSERT
        result.ShouldHaveValidationErrorFor(x => x.ParentLocationId)
              .WithErrorMessage("The specified parent location does not exist.");
    }
    
    [Fact]
    public async Task Validation_Should_Fail_WhenParentIdIsSameAsId()
    {
        // ARRANGE
        var validator = new UpdateLocationCommandValidator(_context);
        var locationId = Guid.NewGuid();
        var command = new UpdateLocationCommand
        {
            Id = locationId,
            Name = "A name",
            ParentLocationId = locationId
        };
        
        // ACT
        var result = await validator.TestValidateAsync(command);

        // ASSERT
        result.ShouldHaveValidationErrorFor(x => x.ParentLocationId)
              .WithErrorMessage("A location cannot be its own parent.");
    }
    
    [Fact]
    public async Task Validation_Should_Succeed_WithValidData()
    {
        // ARRANGE
        var validator = new UpdateLocationCommandValidator(_context);
        var establishment = await CreateEstablishment();
        var location = new Location { EstablishmentId = establishment.Id, Name = "Existing Location" };
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();
        
        var command = new UpdateLocationCommand
        {
            Id = location.Id,
            Name = "Updated Name"
        };
        
        // ACT
        var result = await validator.TestValidateAsync(command);
        
        // ASSERT
        result.ShouldNotHaveAnyValidationErrors();
    }
}