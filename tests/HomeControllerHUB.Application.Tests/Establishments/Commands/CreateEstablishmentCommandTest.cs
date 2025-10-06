using AutoMapper;
using HomeControllerHUB.Application.Establishments.Commands.CreateEstablishment;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Globalization;
using Moq;
using FluentAssertions;
using FluentValidation.TestHelper;
using HomeControllerHUB.Application.Tests.Utils;
using HomeControllerHUB.Domain.Models;

namespace HomeControllerHUB.Application.Tests.Establishments.Commands;

public class CreateEstablishmentCommandTest : TestConfigs
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ISharedResource> _resourceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CreateEstablishmentCommandValidator _validator;
    
    public CreateEstablishmentCommandTest()
    {
        _mapperMock = new Mock<IMapper>();
        _resourceMock = new Mock<ISharedResource>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _validator = new CreateEstablishmentCommandValidator();
    }

    [Fact]
    public async Task Create_Should_Succeed_WithValidParameters()
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

        var command = new CreateEstablishmentCommand
        {
            Name = "Estabelecimento teste",
            SiteName = "Estabelecimento local",
            Document  = "10923812129038",
            Enable = true,
            IsMaster = true,
        };
        
        var handler = new CreateEstablishmentCommandHandler(_context, _mapperMock.Object, _resourceMock.Object, _currentUserServiceMock.Object);

        // ACT
        var result = await handler.Handle(command, CancellationToken.None);

        // ASSERT
        var establishmentInDb = await _context.Establishments.FindAsync(result.Id);
        establishmentInDb.Should().NotBeNull();
        establishmentInDb.Name.Should().Be(command.Name);
    }
    
    [Fact]
    public async Task Create_Should_Fail_WithInvalidDocument()
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

        var command = new CreateEstablishmentCommand
        {
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
    public async Task Create_Should_Fail_WithInvalidName()
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

        var command = new CreateEstablishmentCommand
        {
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
    public async Task Create_Should_Fail_WithInvalidSiteName()
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

        var command = new CreateEstablishmentCommand
        {
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
    public async Task Create_Should_ThrowAppError_WithInvalidUser()
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

        var command = new CreateEstablishmentCommand
        {
            Name = "Estabelecimento teste",
            SiteName = "Estabelecimento local",
            Document  = "10923812129038",
            Enable = true,
            IsMaster = true,
        };
        
        var handler = new CreateEstablishmentCommandHandler(_context, _mapperMock.Object, _resourceMock.Object, _currentUserServiceMock.Object);

        // ACT
        Func<Task> result = async () => await handler.Handle(command, CancellationToken.None);
        
        // ASSERT
        result.Should()
            .ThrowAsync<AppError>()
            .Where(ex => ex.StatusCode == 404);
    }
}