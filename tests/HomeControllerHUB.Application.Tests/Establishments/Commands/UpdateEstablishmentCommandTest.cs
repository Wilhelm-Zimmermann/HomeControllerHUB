using AutoMapper;
using HomeControllerHUB.Application.Establishments.Commands.CreateEstablishment;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Globalization;
using Moq;
using FluentAssertions;
using FluentValidation.TestHelper;
using HomeControllerHUB.Application.Establishments.Commands.UpdateEstablishment;
using HomeControllerHUB.Application.Tests.Utils;
using HomeControllerHUB.Domain.Models;

namespace HomeControllerHUB.Application.Tests.Establishments.Commands;

public class UpdateEstablishmentCommandTest : TestConfigs
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ISharedResource> _resourceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UpdateEstablishmentCommandValidator _validator;
    
    public UpdateEstablishmentCommandTest()
    {
        _mapperMock = new Mock<IMapper>();
        _resourceMock = new Mock<ISharedResource>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _validator = new UpdateEstablishmentCommandValidator();
    }

    [Fact]
    public async Task Update_Should_Succeed_WithValidParameters()
    {
        // ARRANGE
        var authUserId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(authUserId);

        var newEstablishment = await CreateEstablishment();

        var user = UserHelpers.GetApplicationUser();
        user.Id = authUserId;
        user.EstablishmentId = newEstablishment.Id;
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var command = new UpdateEstablishmentCommand
        {
            Id = newEstablishment.Id,
            Name = "Estabelecimento atualizado",
            SiteName = "Estabelecimento local",
            Document  = "10923812129038",
            Enable = true,
            IsMaster = true,
        };
        
        var handler = new UpdateEstablishmentCommandHandler(_context, _resourceMock.Object, _mapperMock.Object, _currentUserServiceMock.Object);

        // ACT
        await handler.Handle(command, CancellationToken.None);

        // ASSERT
        var establishmentInDb = await _context.Establishments.FindAsync(newEstablishment.Id);
        
        establishmentInDb.Should().NotBeNull();
        establishmentInDb.Name.Should().Be(command.Name);
    }
    
    [Fact]
    public async Task Update_Should_Fail_WithNonExistingEstablishment()
    {
        // ARRANGE
        var authUserId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(authUserId);

        var newEstablishment = await CreateEstablishment();

        var user = UserHelpers.GetApplicationUser();
        user.Id = authUserId;
        user.EstablishmentId = newEstablishment.Id;
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var command = new UpdateEstablishmentCommand
        {
            Id = newEstablishment.Id,
            Name = "Estabelecimento atualizado",
            SiteName = "Estabelecimento local",
            Document  = "10923812129038",
            Enable = true,
            IsMaster = true,
        };
        
        var handler = new UpdateEstablishmentCommandHandler(_context, _resourceMock.Object, _mapperMock.Object, _currentUserServiceMock.Object);

        // ACT
        await handler.Handle(command, CancellationToken.None);

        // ASSERT
        var establishmentInDb = await _context.Establishments.FindAsync(new Guid("cdecd467-9729-42d5-8694-9189f1a7a234"));
        
        establishmentInDb.Should().BeNull();
    }
    
    [Fact]
    public async Task Update_Should_Fail_WithInvalidDocument()
    {
        // ARRANGE
        var authUserId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(authUserId);

        var newEstablishment = await CreateEstablishment();

        var user = UserHelpers.GetApplicationUser();
        user.Id = authUserId;
        user.EstablishmentId = newEstablishment.Id;
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var command = new UpdateEstablishmentCommand
        {
            Id = Guid.NewGuid(),
            Name = "Estabelecimento teste",
            SiteName = "Estabelecimento local",
            Enable = true,
            IsMaster = true,
        };
        
        // ACT
        var result = _validator.TestValidate(command);

        // ASSERT
        result.ShouldHaveValidationErrorFor(command => command.Document);
    }
        
    [Fact]
    public async Task Update_Should_Fail_WithInvalidName()
    {
        // ARRANGE
        var authUserId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(authUserId);

        var newEstablishment = await CreateEstablishment();

        var user = UserHelpers.GetApplicationUser();
        user.Id = authUserId;
        user.EstablishmentId = newEstablishment.Id;
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var command = new UpdateEstablishmentCommand
        {
            Id = Guid.NewGuid(),
            SiteName = "Estabelecimento local",
            Document  = "10923812129038",
            Enable = true,
            IsMaster = true,
        };
        
        // ACT
        var result = _validator.TestValidate(command);

        // ASSERT
        result.ShouldHaveValidationErrorFor(command => command.Name);
    }
            
    [Fact]
    public async Task Update_Should_Fail_WithInvalidSiteName()
    {
        // ARRANGE
        var authUserId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(authUserId);

        var newEstablishment = await CreateEstablishment();

        var user = UserHelpers.GetApplicationUser();
        user.Id = authUserId;
        user.EstablishmentId = newEstablishment.Id;
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var command = new UpdateEstablishmentCommand
        {
            Id = Guid.NewGuid(),
            Name = "Estabelecimento",
            Document  = "10923812129038",
            Enable = true,
            IsMaster = true,
        };
        
        // ACT
        var result = _validator.TestValidate(command);

        // ASSERT
        result.ShouldHaveValidationErrorFor(command => command.SiteName);
    }
    
    [Fact]
    public async Task Update_Should_Fail_WithInvalidId()
    {
        // ARRANGE
        var authUserId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(authUserId);

        var newEstablishment = await CreateEstablishment();

        var user = UserHelpers.GetApplicationUser();
        user.Id = authUserId;
        user.EstablishmentId = newEstablishment.Id;
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var command = new UpdateEstablishmentCommand
        {
            Name = "Estabelecimento",
            SiteName = "Estabelecimento saite",
            Document  = "10923812129038",
            Enable = true,
            IsMaster = true,
        };
        
        // ACT
        var result = _validator.TestValidate(command);

        // ASSERT
        result.ShouldHaveValidationErrorFor(command => command.Id);
    }
    
    [Fact]
    public async Task Update_Should_Fail_WithInvalidUser()
    {
        // ARRANGE
        var authUserId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(Guid.Empty);

        var newEstablishment = await CreateEstablishment();

        var user = UserHelpers.GetApplicationUser();
        user.Id = authUserId;
        user.EstablishmentId = newEstablishment.Id;
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var command = new UpdateEstablishmentCommand
        {
            Id = Guid.NewGuid(),
            Name = "Estabelecimento teste",
            SiteName = "Estabelecimento local",
            Document  = "10923812129038",
            Enable = true,
            IsMaster = true,
        };
        
        var handler = new UpdateEstablishmentCommandHandler(_context, _resourceMock.Object, _mapperMock.Object, _currentUserServiceMock.Object);

        // ACT
        Func<Task> result = async () => await handler.Handle(command, CancellationToken.None);
        
        // ASSERT
        result.Should()
            .ThrowAsync<AppError>()
            .Where(ex => ex.StatusCode == 404);
    }
}