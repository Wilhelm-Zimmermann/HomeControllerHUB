using AutoMapper;
using FluentAssertions;
using HomeControllerHUB.Application.Sensors.Queries;
using HomeControllerHUB.Application.Sensors.Queries.GetSensorList;
using HomeControllerHUB.Domain.Entities;

namespace HomeControllerHUB.Application.Tests.Sensors.Queries;

public class GetSensorListQueryTest : TestConfigs
{
    private readonly IMapper _mapper;

    public GetSensorListQueryTest()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Sensor, SensorDto>();
        });
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Get_Should_ReturnFilteredList()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var location1 = new Location { Name = "Living Room", EstablishmentId = establishment.Id };
        _context.Locations.Add(location1);
        
        _context.Sensors.AddRange(
            new Sensor { Name = "LR Temp", DeviceId = "D1", Model = "M", Type = SensorType.Temperature, IsActive = true, EstablishmentId = establishment.Id, Location = location1 },
            new Sensor { Name = "LR Motion", DeviceId = "D2", Model = "M", Type = SensorType.Motion, IsActive = false, EstablishmentId = establishment.Id, Location = location1 },
            new Sensor { Name = "Garage Temp", DeviceId = "D3", Model = "M", Type = SensorType.Temperature, IsActive = true, EstablishmentId = establishment.Id, Location = new Location{Name = "Garage", EstablishmentId = establishment.Id} }
        );
        await _context.SaveChangesAsync();

        var query = new GetSensorListQuery { LocationId = location1.Id, IsActive = true };
        var handler = new GetSensorListQueryHandler(_context, _mapper);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("LR Temp");
    }
}
