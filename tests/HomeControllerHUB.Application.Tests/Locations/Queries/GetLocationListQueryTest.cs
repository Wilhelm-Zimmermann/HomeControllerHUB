using AutoMapper;
using FluentAssertions;
using HomeControllerHUB.Application.Locations.Queries;
using HomeControllerHUB.Application.Locations.Queries.GetLocationList;
using HomeControllerHUB.Domain.Entities;
using Moq;

namespace HomeControllerHUB.Application.Tests.Locations.Queries;

public class GetLocationListQueryTest : TestConfigs
{
    private readonly Mock<IMapper> _mapperMock;

    public GetLocationListQueryTest()
    {
        _mapperMock = new Mock<IMapper>();
    }

    [Fact]
    public async Task Get_Should_ReturnFilteredByEstablishmentId()
    {
        // ARRANGE
        var establishment1 = await CreateEstablishment("Est 1");
        var establishment2 = await CreateEstablishment("Est 2");
        
        _context.Locations.AddRange(
            new Location { EstablishmentId = establishment1.Id, Name = "Loc A" },
            new Location { EstablishmentId = establishment1.Id, Name = "Loc B" },
            new Location { EstablishmentId = establishment2.Id, Name = "Loc C" }
        );
        await _context.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map<List<LocationDto>>(It.IsAny<List<Location>>()))
            .Returns<List<Location>>(locs => locs.Select(l => new LocationDto { Id = l.Id }).ToList());
        
        var query = new GetLocationListQuery { EstablishmentId = establishment1.Id };
        var handler = new GetLocationListQueryHandler(_context, _mapperMock.Object);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Get_Should_ReturnFilteredByParentLocationId()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var parent = new Location { EstablishmentId = establishment.Id, Name = "Parent" };
        _context.Locations.AddRange(
            parent,
            new Location { EstablishmentId = establishment.Id, Name = "Child 1", ParentLocation = parent },
            new Location { EstablishmentId = establishment.Id, Name = "Child 2", ParentLocation = parent },
            new Location { EstablishmentId = establishment.Id, Name = "Root Level" }
        );
        await _context.SaveChangesAsync();
        
        _mapperMock.Setup(m => m.Map<List<LocationDto>>(It.IsAny<List<Location>>()))
            .Returns<List<Location>>(locs => locs.Select(l => new LocationDto { Id = l.Id }).ToList());

        var query = new GetLocationListQuery { ParentLocationId = parent.Id };
        var handler = new GetLocationListQueryHandler(_context, _mapperMock.Object);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Should().HaveCount(2);
    }
}