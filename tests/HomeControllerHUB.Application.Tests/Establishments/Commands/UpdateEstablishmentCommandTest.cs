using AutoMapper;
using HomeControllerHUB.Application.Establishments.Commands.CreateEstablishment;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Globalization;
using Moq;
using FluentAssertions;
using FluentValidation.TestHelper;
using HomeControllerHUB.Application.Establishments.Commands.UpdateEstablishment;
using HomeControllerHUB.Application.Tests.Utils;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Models;
using Microsoft.EntityFrameworkCore;

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
    public async Task Update_Should_Not_Change_UserLinks_WhenUserIdsIsNull()
    {
        // ARRANGE
        var authUserId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(authUserId);

        var establishment = await CreateEstablishment();
        var linkedUser = UserHelpers.GetApplicationUser();
        linkedUser.Id = Guid.NewGuid();
        linkedUser.EstablishmentId = establishment.Id;

        _context.Users.Add(linkedUser);
        _context.UserEstablishments.Add(new UserEstablishment
        {
            EstablishmentId = establishment.Id,
            User = linkedUser
        });
        await _context.SaveChangesAsync();

        var command = new UpdateEstablishmentCommand
        {
            Id = establishment.Id,
            Name = "Estabelecimento atualizado",
            SiteName = "Estabelecimento local",
            Document = "10923812129038",
            Enable = true,
            IsMaster = true,
            UserIds = null
        };

        var handler = new UpdateEstablishmentCommandHandler(_context, _resourceMock.Object, _mapperMock.Object, _currentUserServiceMock.Object);

        // ACT
        await handler.Handle(command, CancellationToken.None);

        // ASSERT
        var userLinks = await _context.UserEstablishments
            .Where(userEstablishment => userEstablishment.EstablishmentId == establishment.Id)
            .ToListAsync();

        userLinks.Should().ContainSingle();
        userLinks[0].UserId.Should().Be(linkedUser.Id);
    }

    [Fact]
    public async Task Update_Should_Replace_UserLinks_WhenUserIdsIsProvided()
    {
        // ARRANGE
        var authUserId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(authUserId);

        var establishment = await CreateEstablishment();
        var previousUser = UserHelpers.GetApplicationUser();
        previousUser.Id = Guid.NewGuid();
        previousUser.EstablishmentId = establishment.Id;
        previousUser.Name = "Previous User";
        previousUser.Email = "previous@test.com";
        previousUser.Login = "previous";
        previousUser.UserName = "previous";
        previousUser.NormalizedEmail = "PREVIOUS@TEST.COM";
        previousUser.NormalizedUserName = "PREVIOUS";

        var authUser = UserHelpers.GetApplicationUser();
        authUser.Id = authUserId;
        authUser.EstablishmentId = establishment.Id;
        authUser.Name = "Auth User";
        authUser.Email = "auth@test.com";
        authUser.Login = "auth";
        authUser.UserName = "auth";
        authUser.NormalizedEmail = "AUTH@TEST.COM";
        authUser.NormalizedUserName = "AUTH";

        var selectedUser = UserHelpers.GetApplicationUser();
        selectedUser.Id = Guid.NewGuid();
        selectedUser.EstablishmentId = establishment.Id;
        selectedUser.Name = "Selected User";
        selectedUser.Email = "selected@test.com";
        selectedUser.Login = "selected";
        selectedUser.UserName = "selected";
        selectedUser.NormalizedEmail = "SELECTED@TEST.COM";
        selectedUser.NormalizedUserName = "SELECTED";

        _context.Users.AddRange(previousUser, authUser, selectedUser);
        _context.UserEstablishments.Add(new UserEstablishment
        {
            EstablishmentId = establishment.Id,
            User = previousUser
        });
        await _context.SaveChangesAsync();

        var command = new UpdateEstablishmentCommand
        {
            Id = establishment.Id,
            Name = "Estabelecimento atualizado",
            SiteName = "Estabelecimento local",
            Document = "10923812129038",
            Enable = true,
            IsMaster = true,
            UserIds = new List<Guid> { selectedUser.Id }
        };

        var handler = new UpdateEstablishmentCommandHandler(_context, _resourceMock.Object, _mapperMock.Object, _currentUserServiceMock.Object);

        // ACT
        await handler.Handle(command, CancellationToken.None);

        // ASSERT
        var linkedUserIds = await _context.UserEstablishments
            .Where(userEstablishment => userEstablishment.EstablishmentId == establishment.Id)
            .Select(userEstablishment => userEstablishment.UserId)
            .ToListAsync();

        linkedUserIds.Should().BeEquivalentTo(new[] { selectedUser.Id, authUserId });
        linkedUserIds.Should().NotContain(previousUser.Id);
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
