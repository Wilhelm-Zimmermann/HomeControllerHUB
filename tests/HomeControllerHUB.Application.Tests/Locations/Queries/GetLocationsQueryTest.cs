using AutoMapper;
using FluentAssertions;
using FluentValidation.TestHelper;
using HomeControllerHUB.Application.Locations.Queries;
using HomeControllerHUB.Application.Locations.Queries.GetLocations;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Models;
using Moq;

namespace HomeControllerHUB.Application.Tests.Locations.Queries;

public class GetLocationsQueryTest : TestConfigs
{
    private readonly Mock<IMapper> _mapperMock;

    public GetLocationsQueryTest()
    {
        _mapperMock = new Mock<IMapper>();
    }

    [Fact]
    public async Task Get_Should_ReturnPaginatedAllLocations()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        for (int i = 0; i < 15; i++)
        {
            _context.Locations.Add(new Location { EstablishmentId = establishment.Id, Name = $"Root {i + 1}" });
        }

        var parentLocation = new Location { EstablishmentId = establishment.Id, Name = "Parent" };
        _context.Locations.Add(parentLocation);
        await _context.SaveChangesAsync();

        _context.Locations.Add(new Location
        {
            EstablishmentId = establishment.Id,
            Name = "Child",
            ParentLocationId = parentLocation.Id,
        });
        await _context.SaveChangesAsync();
        
        _mapperMock.Setup(m => m.Map<List<LocationDto>>(It.IsAny<List<Location>>()))
            .Returns<List<Location>>(locs => locs.Select(l => new LocationDto { Id = l.Id }).ToList());

        var query = new GetLocationsQuery { EstablishmentId = establishment.Id, PageNumber = 2, PageSize = 10 };
        var handler = new GetLocationsQueryHandler(_context, _mapperMock.Object);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(7);
        result.TotalCount.Should().Be(17);
        result.PageNumber.Should().Be(2);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public async Task Get_Should_ReturnOnlyRootLocations_WhenRootOnlyIsTrue()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var parentLocation = new Location { EstablishmentId = establishment.Id, Name = "Parent" };
        _context.Locations.Add(parentLocation);
        await _context.SaveChangesAsync();

        _context.Locations.Add(new Location
        {
            EstablishmentId = establishment.Id,
            Name = "Child",
            ParentLocationId = parentLocation.Id,
        });
        await _context.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map<List<LocationDto>>(It.IsAny<List<Location>>()))
            .Returns<List<Location>>(locs => locs.Select(l => new LocationDto { Id = l.Id }).ToList());

        var query = new GetLocationsQuery { EstablishmentId = establishment.Id, RootOnly = true };
        var handler = new GetLocationsQueryHandler(_context, _mapperMock.Object);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Items.Should().ContainSingle();
        result.Items.Single().Id.Should().Be(parentLocation.Id);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Get_Should_ReturnOnlyChildren_WhenParentLocationIdIsProvided()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var parentLocation = new Location { EstablishmentId = establishment.Id, Name = "Parent" };
        var otherParentLocation = new Location { EstablishmentId = establishment.Id, Name = "Other Parent" };
        _context.Locations.AddRange(parentLocation, otherParentLocation);
        await _context.SaveChangesAsync();

        var childLocation = new Location
        {
            EstablishmentId = establishment.Id,
            Name = "Child",
            ParentLocationId = parentLocation.Id,
        };
        _context.Locations.AddRange(
            childLocation,
            new Location
            {
                EstablishmentId = establishment.Id,
                Name = "Other Child",
                ParentLocationId = otherParentLocation.Id,
            });
        await _context.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map<List<LocationDto>>(It.IsAny<List<Location>>()))
            .Returns<List<Location>>(locs => locs.Select(l => new LocationDto { Id = l.Id }).ToList());

        var query = new GetLocationsQuery
        {
            EstablishmentId = establishment.Id,
            ParentLocationId = parentLocation.Id,
        };
        var handler = new GetLocationsQueryHandler(_context, _mapperMock.Object);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Items.Should().ContainSingle();
        result.Items.Single().Id.Should().Be(childLocation.Id);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Get_Should_FillParentLocationName_ForChildLocations()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var parentLocation = new Location { EstablishmentId = establishment.Id, Name = "Parent" };
        _context.Locations.Add(parentLocation);
        await _context.SaveChangesAsync();

        var childLocation = new Location
        {
            EstablishmentId = establishment.Id,
            Name = "Child",
            ParentLocationId = parentLocation.Id,
        };
        _context.Locations.Add(childLocation);
        await _context.SaveChangesAsync();

        _mapperMock.Setup(m => m.Map<List<LocationDto>>(It.IsAny<List<Location>>()))
            .Returns<List<Location>>(locs => locs.Select(l => new LocationDto { Id = l.Id }).ToList());

        var query = new GetLocationsQuery { EstablishmentId = establishment.Id };
        var handler = new GetLocationsQueryHandler(_context, _mapperMock.Object);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        var childDto = result.Items.Single(location => location.Id == childLocation.Id);
        childDto.ParentLocationName.Should().Be(parentLocation.Name);
    }

    [Fact]
    public async Task Get_Should_FilterBySearchString()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        _context.Locations.AddRange(
            new Location { EstablishmentId = establishment.Id, Name = "Living Room", NormalizedName = "LIVING ROOM" },
            new Location { EstablishmentId = establishment.Id, Name = "Dining Room", NormalizedName = "DINING ROOM" },
            new Location { EstablishmentId = establishment.Id, Name = "Office", NormalizedName = "OFFICE" }
        );
        await _context.SaveChangesAsync();
        
        _mapperMock.Setup(m => m.Map<List<LocationDto>>(It.IsAny<List<Location>>()))
            .Returns<List<Location>>(locs => locs.Select(l => new LocationDto { Id = l.Id }).ToList());

        var query = new GetLocationsQuery { EstablishmentId = establishment.Id, SearchString = "ROOM" };
        var handler = new GetLocationsQueryHandler(_context, _mapperMock.Object);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validation_Should_Fail_ForInvalidPageNumber(int pageNumber)
    {
        // ARRANGE
        var validator = new GetLocationsQueryValidator(_context);
        var query = new GetLocationsQuery { PageNumber = pageNumber, PageSize = 10 };

        // ACT
        var result = await validator.TestValidateAsync(query);

        // ASSERT
        result.ShouldHaveValidationErrorFor(x => x.PageNumber);
    }
    
    [Fact]
    public async Task Validation_Should_Fail_WhenEstablishmentDoesNotExist()
    {
        // ARRANGE
        var validator = new GetLocationsQueryValidator(_context);
        var query = new GetLocationsQuery { EstablishmentId = Guid.NewGuid() };

        // ACT
        var result = await validator.TestValidateAsync(query);

        // ASSERT
        result.ShouldHaveValidationErrorFor(x => x.EstablishmentId);
    }
}
