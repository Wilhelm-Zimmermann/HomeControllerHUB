using AutoMapper;
using HomeControllerHUB.Application.Establishments.Commands.CreateEstablishment;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Globalization;
using Moq;
using FluentAssertions;

namespace HomeControllerHUB.Application.Tests.Establishments.Commands;

public class CreateEstablishmentCommandTest : TestConfigs
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ISharedResource> _resourceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    
    public CreateEstablishmentCommandTest()
    {
        _mapperMock = new Mock<IMapper>();
        _resourceMock = new Mock<ISharedResource>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Create_Should_Succeed_WithValidParameters()
    {
        // ARRANGE
        var authUserId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(authUserId);

        var newEstablishment = new Establishment
        {
            Id = Guid.NewGuid(),
            Name = "Estabelecimento teste",
            SiteName = "Estabelecimento local",
            Document  = "10923812129038",
            Enable = true,
            IsMaster = true,
        };
        
        _context.Establishments.Add(newEstablishment);
        await _context.SaveChangesAsync();

        var user = new ApplicationUser()
        {
            Id = authUserId,
            EstablishmentId = newEstablishment.Id,
            Name = "teste",
            Email = "teste@gmai.com",
            Login = "testekkkkk",
            EmailConfirmed = true,
            Enable = true,
            UserName = "lsldkfj",
            Code = "UserShared",
            NormalizedName = "TESTE",
            PasswordHash = "thisisahashdonotignore"
        };
        
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
        
        var handler = new CreateProfileCommandHandler(_context, _mapperMock.Object, _resourceMock.Object, _currentUserServiceMock.Object);

        // ACT
        var result = await handler.Handle(command, CancellationToken.None);

        // ASSERT
        var establishmentInDb = await _context.Establishments.FindAsync(result.Id);
        
        establishmentInDb.Should().NotBeNull();
        establishmentInDb.Name.Should().Be(command.Name);
    }
}