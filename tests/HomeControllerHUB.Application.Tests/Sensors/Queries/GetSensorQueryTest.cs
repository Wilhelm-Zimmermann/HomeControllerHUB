using AutoMapper;
using FluentAssertions;
using HomeControllerHUB.Application.Sensors.Queries;
using HomeControllerHUB.Application.Sensors.Queries.GetSensor;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using Moq;

namespace HomeControllerHUB.Application.Tests.Sensors.Queries;

public class GetSensorQueryTest : TestConfigs
{
    private readonly IMapper _mapper;
    private readonly Mock<ISharedResource> _resourceMock;

    public GetSensorQueryTest()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Sensor, SensorDto>();
        });
        _mapper = config.CreateMapper();
        _resourceMock = new Mock<ISharedResource>();
    }

    [Fact]
    public async Task Get_Should_ReturnSensor_WhenIdExists()
    {
        // ARRANGE
        var establishment = await CreateEstablishment();
        var location = new Location { EstablishmentId = establishment.Id, Name = "L" };
        var sensor = new Sensor { Name = "N", DeviceId = "D", Model = "M", Type = SensorType.Door, EstablishmentId = establishment.Id, Location = location };
        _context.Sensors.Add(sensor);
        await _context.SaveChangesAsync();

        var query = new GetSensorQuery { Id = sensor.Id };
        var handler = new GetSensorQueryHandler(_context, _mapper, _resourceMock.Object);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        result.Should().NotBeNull();
        result.Id.Should().Be(sensor.Id);
        result.Name.Should().Be(sensor.Name);
    }

    [Fact]
    public async Task Get_Should_ThrowAppError_WhenIdDoesNotExist()
    {
        // ARRANGE
        var query = new GetSensorQuery { Id = Guid.NewGuid() };
        var handler = new GetSensorQueryHandler(_context, _mapper, _resourceMock.Object);
        _resourceMock.Setup(r => r.NotFoundMessage(It.IsAny<string>())).Returns("Not found");
        _resourceMock.Setup(r => r.Message(It.IsAny<string>())).Returns("Error message");

        // ACT
        Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

        // ASSERT
        await act.Should().ThrowAsync<AppError>().Where(e => e.StatusCode == 404);
    }
}
