using AutoMapper;
using FluentAssertions;
using HomeControllerHUB.Application.Profiles.Queries.GetProfileSelector;
using Profile = HomeControllerHUB.Domain.Entities.Profile;

namespace HomeControllerHUB.Application.Tests.Profiles.Queries;

public class GetProfileSelectorQueryTest : TestConfigs
{
    private readonly IMapper _mapper;

    public GetProfileSelectorQueryTest()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Profile, ProfileSelectorDto>();
        });
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Get_Should_ReturnAllProfilesAsSelectorDto()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        _context.Profiles.AddRange(
            new Profile { Name = "Profile A", EstablishmentId = establishment.Id },
            new Profile { Name = "Profile B", EstablishmentId = establishment.Id }
        );
        await _context.SaveChangesAsync();
        
        var query = new GetProfileSelectorQuery();
        var handler = new GetProfileSelectorQueryHandler(_context, _mapper);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Should().BeOfType<ProfileSelectorDto>();
    }
}
