using AutoMapper;
using FluentAssertions;
using HomeControllerHUB.Application.Profiles.Queries;
using HomeControllerHUB.Application.Profiles.Queries.GetAllProfilePaginated;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Mappings;
using Moq;
using Profile = HomeControllerHUB.Domain.Entities.Profile;

namespace HomeControllerHUB.Application.Tests.Profiles.Queries;

public class GetAllProfilePaginatedQueryTest : TestConfigs
{
    private readonly IMapper _mapper;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public GetAllProfilePaginatedQueryTest()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new MappingProfile(typeof(GetProfilePaginatedDto).Assembly));
        });
        _mapper = config.CreateMapper();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Get_Should_ReturnPaginatedList_FilteredByCurrentUserEstablishment()
    {
        // ARRANGE
        var targetEstablishmentId = (await CreateEstablishment()).Id;
        var otherEstablishmentId = (await CreateEstablishment()).Id;
        _currentUserServiceMock.Setup(s => s.EstablishmentId).Returns(targetEstablishmentId);

        for (int i = 0; i < 15; i++)
        {
            _context.Profiles.Add(new Profile { Name = $"Profile {i}", Description = $"Description {i}", Enable = i % 2 == 0, EstablishmentId = targetEstablishmentId });
        }
        
        for (int i = 0; i < 5; i++)
        {
            _context.Profiles.Add(new Profile { Name = $"Other Profile {i}", EstablishmentId = otherEstablishmentId });
        }
        await _context.SaveChangesAsync();

        var query = new GetAllProfilePaginatedQuery { PageNumber = 2, PageSize = 10 };
        var handler = new GetAllProfilePaginatedQueryHandler(_context, _currentUserServiceMock.Object, _mapper);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(2);
    }

    [Fact]
    public async Task Get_Should_FilterBySearchBy()
    {
        // ARRANGE
        var establishmentId = (await CreateEstablishment()).Id;
        _currentUserServiceMock.Setup(s => s.EstablishmentId).Returns(establishmentId);

        _context.Profiles.AddRange(
            new Profile { Name = "Administrator Profile", NormalizedName = "ADMINISTRATOR PROFILE", EstablishmentId = establishmentId },
            new Profile { Name = "Standard User Profile", NormalizedName = "STANDARD USER PROFILE", EstablishmentId = establishmentId },
            new Profile { Name = "Guest User", NormalizedName = "GUEST USER", EstablishmentId = establishmentId }
        );
        await _context.SaveChangesAsync();

        var query = new GetAllProfilePaginatedQuery { SearchBy = "Profile" };
        var handler = new GetAllProfilePaginatedQueryHandler(_context, _currentUserServiceMock.Object, _mapper);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Get_Should_FilterByEnable()
    {
        // ARRANGE
        var establishmentId = (await CreateEstablishment()).Id;
        _currentUserServiceMock.Setup(s => s.EstablishmentId).Returns(establishmentId);

        _context.Profiles.AddRange(
            new Profile { Name = "Enabled Profile", Enable = true, EstablishmentId = establishmentId },
            new Profile { Name = "Disabled Profile", Enable = false, EstablishmentId = establishmentId }
        );
        await _context.SaveChangesAsync();

        var query = new GetAllProfilePaginatedQuery { Enable = true };
        var handler = new GetAllProfilePaginatedQueryHandler(_context, _currentUserServiceMock.Object, _mapper);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Items.Should().HaveCount(1);
        result.Items.First().Enable.Should().BeTrue();
    }

    [Fact]
    public async Task Get_Should_ReturnUsersAndPrivilegesCount()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        _currentUserServiceMock.Setup(s => s.EstablishmentId).Returns(establishment.Id);

        var domain = new HomeControllerHUB.Domain.Entities.ApplicationDomain { Name = "Profile Domain", Description = "Profiles" };
        var privilege = new HomeControllerHUB.Domain.Entities.Privilege { Name = "profile-read", Description = "Read", Actions = "Read", Domain = domain, EstablishmentId = establishment.Id };
        var profile = new Profile { Name = "Admin", EstablishmentId = establishment.Id };
        var user = new HomeControllerHUB.Domain.Entities.ApplicationUser { Name = "User", Login = "user", Email = "user@test.com", PasswordHash = "hash", EstablishmentId = establishment.Id };

        _context.Domains.Add(domain);
        _context.Privilege.Add(privilege);
        _context.Profiles.Add(profile);
        _context.Users.Add(user);
        _context.ProfilePrivileges.Add(new HomeControllerHUB.Domain.Entities.ProfilePrivilege { Profile = profile, Privilege = privilege });
        _context.UserProfiles.Add(new HomeControllerHUB.Domain.Entities.UserProfile { Profile = profile, User = user });
        await _context.SaveChangesAsync();

        var query = new GetAllProfilePaginatedQuery();
        var handler = new GetAllProfilePaginatedQueryHandler(_context, _currentUserServiceMock.Object, _mapper);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Items.Should().ContainSingle();
        result.Items.First().UsersCount.Should().Be(1);
        result.Items.First().PrivilegesCount.Should().Be(1);
    }
}
