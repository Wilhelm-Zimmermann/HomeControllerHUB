using AutoMapper;
using FluentAssertions;
using HomeControllerHUB.Application.Profiles.Queries;
using HomeControllerHUB.Application.Profiles.Queries.GetProfileById;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Mappings;
using Moq;
using Profile = HomeControllerHUB.Domain.Entities.Profile;

namespace HomeControllerHUB.Application.Tests.Profiles.Queries;

public class GetProfileByIdQueryTest : TestConfigs
{
    private readonly IMapper _mapper;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public GetProfileByIdQueryTest()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new MappingProfile(typeof(ProfileDto).Assembly));
        });
        _mapper = config.CreateMapper();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Get_Should_ReturnProfile_WhenIdExists()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        _currentUserServiceMock.Setup(s => s.EstablishmentId).Returns(establishment.Id);
        var profile = new Profile { Name = "Test Profile", EstablishmentId = establishment.Id };
        _context.Profiles.Add(profile);
        await _context.SaveChangesAsync();

        var query = new GetProfileByIdQuery(profile.Id);
        var handler = new GetProfileByIdQueryHandler(_context, _mapper, _currentUserServiceMock.Object);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Should().NotBeNull();
        result.Id.Should().Be(profile.Id);
        result.Name.Should().Be(profile.Name);
    }

    [Fact]
    public async Task Get_Should_ReturnNull_WhenIdDoesNotExist()
    {
        // ARRANGE
        _currentUserServiceMock.Setup(s => s.EstablishmentId).Returns(Guid.NewGuid());
        var query = new GetProfileByIdQuery(Guid.NewGuid());
        var handler = new GetProfileByIdQueryHandler(_context, _mapper, _currentUserServiceMock.Object);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Should().BeNull();
    }

    [Fact]
    public async Task Get_Should_ReturnProfilePrivileges_WhenIdExists()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        _currentUserServiceMock.Setup(s => s.EstablishmentId).Returns(establishment.Id);

        var domain = new ApplicationDomain { Name = "Profile", Description = "Profiles" };
        var privilege = new Privilege
        {
            Name = "profile-read",
            Description = "Read profiles",
            Actions = "Read",
            Domain = domain,
            EstablishmentId = establishment.Id
        };
        var profile = new Profile { Name = "Admin", EstablishmentId = establishment.Id };

        _context.Domains.Add(domain);
        _context.Privilege.Add(privilege);
        _context.Profiles.Add(profile);
        _context.ProfilePrivileges.Add(new ProfilePrivilege { Profile = profile, Privilege = privilege });
        await _context.SaveChangesAsync();

        var query = new GetProfileByIdQuery(profile.Id);
        var handler = new GetProfileByIdQueryHandler(_context, _mapper, _currentUserServiceMock.Object);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.PrivilegeIds.Should().ContainSingle().Which.Should().Be(privilege.Id);
        result.Privileges.Should().ContainSingle();
        result.Privileges.First().PrivilegeId.Should().Be(privilege.Id);
        result.Privileges.First().Domain.Should().Be(domain.Name);
        result.Privileges.First().Action.Should().Be(privilege.Actions);
    }
}
