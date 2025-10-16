using AutoMapper;
using FluentAssertions;
using HomeControllerHUB.Application.Sensors.Queries;
using HomeControllerHUB.Application.Sensors.Queries.GetSensorReadings;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Globalization;
using Moq;

namespace HomeControllerHUB.Application.Tests.Sensors.Queries;

public class GetSensorReadingsQueryTest : TestConfigs
{
    private readonly IMapper _mapper;
    private readonly Mock<ISharedResource> _resourceMock;

    public GetSensorReadingsQueryTest()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SensorReading, SensorReadingDto>();
        });
        _mapper = config.CreateMapper();
        _resourceMock = new Mock<ISharedResource>();
    }

    [Fact]
    public async Task Get_Should_ReturnPaginatedReadings()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var location = new Location { EstablishmentId = establishment.Id, Name = "L" };
        var sensor = new Sensor { Name = "N", DeviceId = "D", Model = "M", Type = SensorType.Door, EstablishmentId = establishment.Id, Location = location };
        _context.Sensors.Add(sensor);

        for (int i = 0; i < 25; i++)
        {
            _context.SensorReadings.Add(new SensorReading { Sensor = sensor, Value = i, Timestamp = DateTime.UtcNow.AddMinutes(-i) });
        }
        await _context.SaveChangesAsync();

        var query = new GetSensorReadingsQuery { SensorId = sensor.Id, PageNumber = 2, PageSize = 10 };
        var handler = new GetSensorReadingsQueryHandler(_context, _mapper, _resourceMock.Object);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.Items.First().Value.Should().Be(10);
    }
}
