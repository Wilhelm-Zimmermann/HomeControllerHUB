using AutoMapper;
using FluentAssertions;
using HomeControllerHUB.Application.Profiles.Queries;
using HomeControllerHUB.Application.Profiles.Queries.GetProfileById;
using Profile = HomeControllerHUB.Domain.Entities.Profile;

namespace HomeControllerHUB.Application.Tests.Profiles.Queries;

public class GetProfileByIdQueryTest : TestConfigs
{
    private readonly IMapper _mapper;

    public GetProfileByIdQueryTest()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Profile, ProfileDto>();
        });
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Get_Should_ReturnProfile_WhenIdExists()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var profile = new Profile { Name = "Test Profile", EstablishmentId = establishment.Id };
        _context.Profiles.Add(profile);
        await _context.SaveChangesAsync();

        var query = new GetProfileByIdQuery(profile.Id);
        var handler = new GetProfileByIdQueryHandler(_context, _mapper);

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
        var query = new GetProfileByIdQuery(Guid.NewGuid());
        var handler = new GetProfileByIdQueryHandler(_context, _mapper);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Should().BeNull();
    }
}
