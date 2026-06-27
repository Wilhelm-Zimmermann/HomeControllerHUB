using AutoMapper;
using FluentAssertions;
using HomeControllerHUB.Application.Profiles.Queries.GetProfileSelector;
using HomeControllerHUB.Domain.Interfaces;
using Moq;
using Profile = HomeControllerHUB.Domain.Entities.Profile;

namespace HomeControllerHUB.Application.Tests.Profiles.Queries;

public class GetProfileSelectorQueryTest : TestConfigs
{
    private readonly IMapper _mapper;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public GetProfileSelectorQueryTest()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Profile, ProfileSelectorDto>();
        });
        _mapper = config.CreateMapper();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Get_Should_ReturnProfilesFromCurrentEstablishmentAsSelectorDto()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var otherEstablishment = await CreateEstablishment("Other establishment");
        _currentUserServiceMock.Setup(s => s.EstablishmentId).Returns(establishment.Id);

        _context.Profiles.AddRange(
            new Profile { Name = "Profile A", EstablishmentId = establishment.Id },
            new Profile { Name = "Profile B", EstablishmentId = establishment.Id },
            new Profile { Name = "Other Profile", EstablishmentId = otherEstablishment.Id }
        );
        await _context.SaveChangesAsync();

        var query = new GetProfileSelectorQuery();
        var handler = new GetProfileSelectorQueryHandler(_context, _mapper, _currentUserServiceMock.Object);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Should().BeOfType<ProfileSelectorDto>();
    }
}
