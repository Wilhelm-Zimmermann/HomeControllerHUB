using AutoMapper;
using FluentAssertions;
using HomeControllerHUB.Application.Sensors.Queries;
using HomeControllerHUB.Application.Sensors.Queries.GetSensors;
using HomeControllerHUB.Domain.Entities;

namespace HomeControllerHUB.Application.Tests.Sensors.Queries;

public class GetSensorsQueryTest : TestConfigs
{
    private readonly IMapper _mapper;

    public GetSensorsQueryTest()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Sensor, SensorDto>();
        });
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Get_Should_ReturnPaginatedAndFilteredSensors()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var location1 = new Location { Name = "Living Room", EstablishmentId = establishment.Id };
        var location2 = new Location { Name = "Kitchen", EstablishmentId = establishment.Id };
        _context.Locations.AddRange(location1, location2);

        for (int i = 0; i < 15; i++)
        {
            _context.Sensors.Add(new Sensor { Name = $"Active Sensor {i}", DeviceId = $"D{i}", Model = "M", Type = SensorType.Door, IsActive = true, EstablishmentId = establishment.Id, Location = location1 });
        }
        _context.Sensors.Add(new Sensor { Name = "Inactive Sensor", DeviceId = "DX", Model = "M", Type = SensorType.Door, IsActive = false, EstablishmentId = establishment.Id, Location = location1 });
        _context.Sensors.Add(new Sensor { Name = "Kitchen Sensor", DeviceId = "DY", Model = "M", Type = SensorType.Door, IsActive = true, EstablishmentId = establishment.Id, Location = location2 });

        await _context.SaveChangesAsync();

        var query = new GetSensorsQuery
        {
            LocationId = location1.Id,
            IsActive = true,
            PageNumber = 2,
            PageSize = 10
        };
        var handler = new GetSensorsQueryHandler(_context, _mapper);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(15);
    }
}
