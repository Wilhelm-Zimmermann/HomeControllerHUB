using AutoMapper;
using FluentAssertions;
using HomeControllerHUB.Application.Profiles.Queries;
using HomeControllerHUB.Application.Profiles.Queries.GetAllProfilePaginated;
using HomeControllerHUB.Domain.Interfaces;
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
            cfg.CreateMap<Profile, GetProfilePaginatedDto>();
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
            _context.Profiles.Add(new Profile { Name = $"Profile {i}", EstablishmentId = targetEstablishmentId });
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
}
