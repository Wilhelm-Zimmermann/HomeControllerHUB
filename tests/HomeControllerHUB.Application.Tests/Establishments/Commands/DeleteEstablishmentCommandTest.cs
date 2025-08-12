using AutoMapper;
using HomeControllerHUB.Application.Establishments.Commands.CreateEstablishment;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Globalization;
using Moq;
using FluentAssertions;
using FluentValidation.TestHelper;
using HomeControllerHUB.Application.Establishments.Commands.DeleteEstablishment;
using HomeControllerHUB.Application.Tests.Utils;
using HomeControllerHUB.Domain.Models;

namespace HomeControllerHUB.Application.Tests.Establishments.Commands;

public class DeleteEstablishmentCommandTest : TestConfigs
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ISharedResource> _resourceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    
    public DeleteEstablishmentCommandTest()
    {
        _mapperMock = new Mock<IMapper>();
        _resourceMock = new Mock<ISharedResource>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Delete_Should_Succeed_WithValidParameters()
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

        var command = new DeleteEstablishmentCommand(newEstablishment.Id);
        
        var handler = new DeleteEstablishmentCommandHandler(_context, _resourceMock.Object);

        // ACT
        await handler.Handle(command, CancellationToken.None);

        // ASSERT
        var establishmentInDb = await _context.Establishments.FindAsync(newEstablishment.Id);
        
        establishmentInDb.Enable.Should().BeFalse();
    }
    
    [Fact]
    public async Task Delete_Should_ThrowAppError_WithInvalidId()
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

        var command = new DeleteEstablishmentCommand(new Guid("cdecd467-9729-42d5-8694-9189f1a7a234"));
        
        var handler = new DeleteEstablishmentCommandHandler(_context, _resourceMock.Object);

        // ACT
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // ASSERT
        act.Should()
            .ThrowAsync<AppError>()
            .Where(x => x.StatusCode == 404);
    }
}