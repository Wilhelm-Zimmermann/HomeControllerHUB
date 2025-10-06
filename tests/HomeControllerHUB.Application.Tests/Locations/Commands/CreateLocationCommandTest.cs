using FluentAssertions;
using FluentValidation.TestHelper;
using HomeControllerHUB.Application.Locations.Commands.CreateLocation;
using HomeControllerHUB.Domain.Entities;

namespace HomeControllerHUB.Application.Tests.Locations.Commands;

public class CreateLocationCommandTest : TestConfigs
{
     [Fact]
    public async Task Create_Should_Succeed_WithValidParameters()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var command = new CreateLocationCommand
        {
            EstablishmentId = establishment.Id,
            Name = "Living Room",
            Description = "Main living area of the house.",
            Type = LocationType.Room,
            ParentLocationId = null
        };
        
        var handler = new CreateLocationCommandHandler(_context);

        // ACT
        var locationId = await handler.Handle(command, CancellationToken.None);

        // ASSERT
        locationId.Should().NotBe(Guid.Empty);
        var locationInDb = await _context.Locations.FindAsync(locationId);
        locationInDb.Should().NotBeNull();
        locationInDb!.Name.Should().Be(command.Name);
        locationInDb.EstablishmentId.Should().Be(establishment.Id);
    }

    [Fact]
    public async Task Create_Should_Succeed_WithValidParentLocation()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        
        var parentLocation = new Location
        {
            EstablishmentId = establishment.Id,
            Name = "First Floor",
            Type = LocationType.Area
        };
        _context.Locations.Add(parentLocation);
        await _context.SaveChangesAsync();

        var command = new CreateLocationCommand
        {
            EstablishmentId = establishment.Id,
            Name = "Kitchen",
            Description = "Kitchen area on the first floor.",
            Type = LocationType.Room,
            ParentLocationId = parentLocation.Id
        };
        
        var handler = new CreateLocationCommandHandler(_context);

        // ACT
        var locationId = await handler.Handle(command, CancellationToken.None);

        // ASSERT
        var locationInDb = await _context.Locations.FindAsync(locationId);
        locationInDb.Should().NotBeNull();
        locationInDb!.ParentLocationId.Should().Be(parentLocation.Id);
    }

    [Fact]
    public async Task Validation_Should_Fail_WhenEstablishmentDoesNotExist()
    {
        // ARRANGE
        var validator = new CreateLocationCommandValidator(_context);
        var command = new CreateLocationCommand
        {
            EstablishmentId = Guid.NewGuid(),
            Name = "Invalid Location"
        };
        
        // ACT
        var result = await validator.TestValidateAsync(command);

        // ASSERT
        result.ShouldHaveValidationErrorFor(x => x.EstablishmentId)
              .WithErrorMessage("The specified establishment does not exist.");
    }
    
    [Fact]
    public async Task Validation_Should_Fail_WhenNameIsEmpty()
    {
        // ARRANGE
        var validator = new CreateLocationCommandValidator(_context);
        var establishment = await CreateEstablishment();
        var command = new CreateLocationCommand
        {
            EstablishmentId = establishment.Id,
            Name = ""
        };
        
        // ACT
        var result = await validator.TestValidateAsync(command);

        // ASSERT
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
    
    [Theory]
    [InlineData(256)]
    [InlineData(300)]
    public async Task Validation_Should_Fail_WhenNameIsTooLong(int nameLength)
    {
        // ARRANGE
        var validator = new CreateLocationCommandValidator(_context);
        var establishment = await CreateEstablishment();
        var command = new CreateLocationCommand
        {
            EstablishmentId = establishment.Id,
            Name = new string('a', nameLength) 
        };
        
        // ACT
        var result = await validator.TestValidateAsync(command);

        // ASSERT
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Validation_Should_Fail_WhenParentLocationDoesNotExist()
    {
        // ARRANGE
        var validator = new CreateLocationCommandValidator(_context);
        var establishment = await CreateEstablishment();
        var command = new CreateLocationCommand
        {
            EstablishmentId = establishment.Id,
            Name = "Some Room",
            ParentLocationId = Guid.NewGuid() 
        };
        
        // ACT
        var result = await validator.TestValidateAsync(command);

        // ASSERT
        result.ShouldHaveValidationErrorFor(x => x.ParentLocationId)
              .WithErrorMessage("The specified parent location does not exist.");
    }
    
    [Fact]
    public async Task Validation_Should_Succeed_WithValidData()
    {
        // ARRANGE
        var validator = new CreateLocationCommandValidator(_context);
        var establishment = await CreateEstablishment();
        var command = new CreateLocationCommand
        {
            EstablishmentId = establishment.Id,
            Name = "Valid Location Name",
            Type = LocationType.Room
        };
        
        // ACT
        var result = await validator.TestValidateAsync(command);
        
        // ASSERT
        result.ShouldNotHaveAnyValidationErrors();
    }
}