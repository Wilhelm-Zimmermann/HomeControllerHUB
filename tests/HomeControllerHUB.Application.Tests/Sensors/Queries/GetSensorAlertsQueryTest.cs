using AutoMapper;
using FluentAssertions;
using HomeControllerHUB.Application.Sensors.Queries;
using HomeControllerHUB.Application.Sensors.Queries.GetSensorAlerts;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Globalization;
using Moq;

namespace HomeControllerHUB.Application.Tests.Sensors.Queries;

public class GetSensorAlertsQueryTest : TestConfigs
{
    private readonly IMapper _mapper;
    private readonly Mock<ISharedResource> _resourceMock;

    public GetSensorAlertsQueryTest()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SensorAlert, SensorAlertDto>();
        });
        _mapper = config.CreateMapper();
        _resourceMock = new Mock<ISharedResource>();
    }

    [Fact]
    public async Task Get_Should_ReturnPaginatedAndFilteredAlerts()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var location = new Location { EstablishmentId = establishment.Id, Name = "L" };
        var sensor = new Sensor { Name = "N", DeviceId = "D", Model = "M", Type = SensorType.Door, EstablishmentId = establishment.Id, Location = location };
        _context.Sensors.Add(sensor);

        _context.SensorAlerts.AddRange(
            new SensorAlert { Sensor = sensor, Message = "M", Type = AlertType.BatteryLow, IsAcknowledged = true, Timestamp = DateTime.UtcNow.AddHours(-2) },
            new SensorAlert { Sensor = sensor, Message = "M", Type = AlertType.ThresholdExceeded, IsAcknowledged = false, Timestamp = DateTime.UtcNow.AddHours(-1) },
            new SensorAlert { Sensor = sensor, Message = "M", Type = AlertType.ThresholdExceeded, IsAcknowledged = false, Timestamp = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        var query = new GetSensorAlertsQuery
        {
            SensorId = sensor.Id,
            AlertType = AlertType.ThresholdExceeded,
            IsAcknowledged = false
        };
        var handler = new GetSensorAlertsQueryHandler(_context, _mapper, _resourceMock.Object);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }
}
