using AutoMapper;
using FluentAssertions;
using FluentValidation.TestHelper;
using HomeControllerHUB.Application.Locations.Queries;
using HomeControllerHUB.Application.Locations.Queries.GetLocation;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using Moq;

namespace HomeControllerHUB.Application.Tests.Locations.Queries;

public class GetLocationQueryTest : TestConfigs
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ISharedResource> _resourceMock;
    private readonly GetLocationQueryValidator _validator;

    public GetLocationQueryTest()
    {
        _mapperMock = new Mock<IMapper>();
        _resourceMock = new Mock<ISharedResource>();
        _validator = new GetLocationQueryValidator();
    }

    [Fact]
    public async Task Get_Should_ReturnLocation_WhenLocationExists()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var location = new Location { Id = Guid.NewGuid(), Establishment = establishment, Name = "Test Location" };
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        var locationDto = new LocationDto { Id = location.Id, Name = location.Name, EstablishmentName = establishment.Name! };
        _mapperMock.Setup(m => m.Map<LocationDto>(It.IsAny<Location>())).Returns(locationDto);

        var query = new GetLocationQuery { Id = location.Id };
        var handler = new GetLocationQueryHandler(_context, _mapperMock.Object, _resourceMock.Object);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Should().NotBeNull();
        result.Id.Should().Be(location.Id);
        result.Name.Should().Be(location.Name);
        result.EstablishmentName.Should().Be(establishment.Name);
    }

    [Fact]
    public async Task Get_Should_ThrowAppError_WhenLocationNotFound()
    {
        // ARRANGE
        var query = new GetLocationQuery { Id = Guid.NewGuid() };
        var handler = new GetLocationQueryHandler(_context, _mapperMock.Object, _resourceMock.Object);
        _resourceMock.Setup(r => r.NotFoundMessage(nameof(Location))).Returns("Location not found.");

        // ACT
        Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

        // ASSERT
        await act.Should().ThrowAsync<AppError>().Where(ex => ex.StatusCode == 404);
    }

    [Fact]
    public void Validation_Should_Fail_WhenIdIsEmpty()
    {
        // ARRANGE
        var query = new GetLocationQuery { Id = Guid.Empty };

        // ACT
        var result = _validator.TestValidate(query);

        // ASSERT
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}