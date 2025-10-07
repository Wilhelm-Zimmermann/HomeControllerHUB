using AutoMapper;
using FluentAssertions;
using HomeControllerHUB.Application.Locations.Queries;
using HomeControllerHUB.Application.Locations.Queries.GetLocationHierarchy;
using HomeControllerHUB.Domain.Entities;
using Moq;

namespace HomeControllerHUB.Application.Tests.Locations.Queries;

public class GetLocationHierarchyQueryTest : TestConfigs
{
    private readonly Mock<IMapper> _mapperMock;

    public GetLocationHierarchyQueryTest()
    {
        _mapperMock = new Mock<IMapper>();
    }

    [Fact]
    public async Task Get_Should_ReturnCorrectHierarchy()
    {
        // ARRANGE
        var establishment1 = await CreateEstablishment("Establishment 1");
        var establishment2 = await CreateEstablishment("Establishment 2");

        var root1 = new Location { Id = Guid.NewGuid(), EstablishmentId = establishment1.Id, Name = "First Floor" };
        var child1 = new Location { Id = Guid.NewGuid(), EstablishmentId = establishment1.Id, Name = "Living Room", ParentLocationId = root1.Id };
        var child2 = new Location { Id = Guid.NewGuid(), EstablishmentId = establishment1.Id, Name = "Kitchen", ParentLocationId = root1.Id };
        var grandChild1 = new Location { Id = Guid.NewGuid(), EstablishmentId = establishment1.Id, Name = "Pantry", ParentLocationId = child2.Id };
        var root2 = new Location { Id = Guid.NewGuid(), EstablishmentId = establishment1.Id, Name = "Second Floor" };
        
        var otherEstLocation = new Location { Id = Guid.NewGuid(), EstablishmentId = establishment2.Id, Name = "Basement" };
        
        var allLocations = new List<Location> { root1, child1, child2, grandChild1, root2, otherEstLocation };
        _context.Locations.AddRange(allLocations);
        await _context.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map<List<LocationHierarchyDto>>(It.IsAny<List<Location>>()))
            .Returns<List<Location>>(locations => locations.Select(l => new LocationHierarchyDto
            {
                Id = l.Id, Name = l.Name, ParentLocationId = l.ParentLocationId
            }).ToList());
        
        var query = new GetLocationHierarchyQuery { EstablishmentId = establishment1.Id };
        var handler = new GetLocationHierarchyQueryHandler(_context, _mapperMock.Object);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Should().HaveCount(2);
        
        var firstFloor = result.First(r => r.Name == "First Floor");
        firstFloor.Children.Should().HaveCount(2);
        
        var kitchen = firstFloor.Children.First(c => c.Name == "Kitchen");
        kitchen.Children.Should().HaveCount(1);
        kitchen.Children.First().Name.Should().Be("Pantry");
    }
}