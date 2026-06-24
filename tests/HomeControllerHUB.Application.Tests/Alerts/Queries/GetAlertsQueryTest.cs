using FluentAssertions;
using FluentValidation.TestHelper;
using HomeControllerHUB.Application.Alerts.Queries.GetAlerts;
using HomeControllerHUB.Domain.Entities;

namespace HomeControllerHUB.Application.Tests.Alerts.Queries;

public class GetAlertsQueryTest : TestConfigs
{
    [Fact]
    public async Task Get_Should_ReturnPaginatedAlertsFromDifferentSensors()
    {
        // ARRANGE
        var (_, firstSensor, secondSensor) = await CreateAlertScenario();
        _context.SensorAlerts.AddRange(
            new SensorAlert
            {
                SensorId = firstSensor.Id,
                Message = "Temperature threshold exceeded",
                Type = AlertType.ThresholdExceeded,
                Timestamp = DateTime.UtcNow.AddMinutes(-10)
            },
            new SensorAlert
            {
                SensorId = secondSensor.Id,
                Message = "Battery is low",
                Type = AlertType.BatteryLow,
                Timestamp = DateTime.UtcNow
            });
        await _context.SaveChangesAsync();

        var handler = new GetAlertsQueryHandler(_context);

        // ACT
        var result = await handler.Handle(new GetAlertsQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

        // ASSERT
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Select(x => x.SensorId).Should().Contain(new[] { firstSensor.Id, secondSensor.Id });
    }

    [Fact]
    public async Task Get_Should_FillSensorLocationAndEstablishmentData()
    {
        // ARRANGE
        var (establishment, firstSensor, _) = await CreateAlertScenario();
        _context.SensorAlerts.Add(new SensorAlert
        {
            SensorId = firstSensor.Id,
            Message = "Door sensor offline",
            Type = AlertType.DeviceOffline,
            Timestamp = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var handler = new GetAlertsQueryHandler(_context);

        // ACT
        var result = await handler.Handle(new GetAlertsQuery(), CancellationToken.None);

        // ASSERT
        var item = result.Items.Should().ContainSingle().Subject;
        item.SensorName.Should().Be(firstSensor.Name);
        item.SensorDeviceId.Should().Be(firstSensor.DeviceId);
        item.SensorType.Should().Be(firstSensor.Type);
        item.LocationId.Should().Be(firstSensor.LocationId);
        item.LocationName.Should().Be(firstSensor.Location.Name);
        item.EstablishmentId.Should().Be(establishment.Id);
        item.EstablishmentName.Should().Be(establishment.Name);
        item.Type.Should().Be(AlertType.DeviceOffline);
        item.TypeName.Should().Be(nameof(AlertType.DeviceOffline));
    }

    [Fact]
    public async Task Get_Should_FilterBySensorAcknowledgementAndType()
    {
        // ARRANGE
        var (_, firstSensor, secondSensor) = await CreateAlertScenario();
        _context.SensorAlerts.AddRange(
            new SensorAlert
            {
                SensorId = firstSensor.Id,
                Message = "Battery is low",
                Type = AlertType.BatteryLow,
                IsAcknowledged = false,
                Timestamp = DateTime.UtcNow
            },
            new SensorAlert
            {
                SensorId = firstSensor.Id,
                Message = "Sensor error acknowledged",
                Type = AlertType.Error,
                IsAcknowledged = true,
                Timestamp = DateTime.UtcNow
            },
            new SensorAlert
            {
                SensorId = secondSensor.Id,
                Message = "Another battery alert",
                Type = AlertType.BatteryLow,
                IsAcknowledged = false,
                Timestamp = DateTime.UtcNow
            });
        await _context.SaveChangesAsync();

        var query = new GetAlertsQuery
        {
            SensorId = firstSensor.Id,
            IsAcknowledged = false,
            Type = nameof(AlertType.BatteryLow)
        };
        var handler = new GetAlertsQueryHandler(_context);

        // ACT
        var result = await handler.Handle(query, CancellationToken.None);

        // ASSERT
        var item = result.Items.Should().ContainSingle().Subject;
        item.SensorId.Should().Be(firstSensor.Id);
        item.IsAcknowledged.Should().BeFalse();
        item.Type.Should().Be(AlertType.BatteryLow);
    }

    [Fact]
    public async Task Get_Should_FilterByLocationAndEstablishment()
    {
        // ARRANGE
        var (establishment, firstSensor, secondSensor) = await CreateAlertScenario();
        _context.SensorAlerts.AddRange(
            new SensorAlert
            {
                SensorId = firstSensor.Id,
                Message = "First location alert",
                Type = AlertType.Error,
                Timestamp = DateTime.UtcNow
            },
            new SensorAlert
            {
                SensorId = secondSensor.Id,
                Message = "Second location alert",
                Type = AlertType.Error,
                Timestamp = DateTime.UtcNow
            });
        await _context.SaveChangesAsync();

        var handler = new GetAlertsQueryHandler(_context);

        // ACT
        var result = await handler.Handle(new GetAlertsQuery
        {
            EstablishmentId = establishment.Id,
            LocationId = firstSensor.LocationId
        }, CancellationToken.None);

        // ASSERT
        var item = result.Items.Should().ContainSingle().Subject;
        item.SensorId.Should().Be(firstSensor.Id);
        item.LocationId.Should().Be(firstSensor.LocationId);
        item.EstablishmentId.Should().Be(establishment.Id);
    }

    [Fact]
    public async Task Get_Should_FilterBySearchAndCreatedRange()
    {
        // ARRANGE
        var (_, firstSensor, secondSensor) = await CreateAlertScenario();
        var oldAlert = new SensorAlert
        {
            SensorId = firstSensor.Id,
            Message = "Ignored battery message",
            Type = AlertType.BatteryLow,
            Timestamp = DateTime.UtcNow.AddDays(-3)
        };
        var recentAlert = new SensorAlert
        {
            SensorId = secondSensor.Id,
            Message = "Kitchen smoke alert",
            Type = AlertType.Error,
            Timestamp = DateTime.UtcNow
        };
        _context.SensorAlerts.AddRange(oldAlert, recentAlert);
        await _context.SaveChangesAsync();

        oldAlert.Created = DateTime.UtcNow.AddDays(-3);
        recentAlert.Created = DateTime.UtcNow.AddMinutes(-5);
        await _context.SaveChangesAsync();

        var handler = new GetAlertsQueryHandler(_context);

        // ACT
        var result = await handler.Handle(new GetAlertsQuery
        {
            Search = "Kitchen",
            CreatedStart = DateTime.UtcNow.AddHours(-1),
            CreatedEnd = DateTime.UtcNow.AddHours(1)
        }, CancellationToken.None);

        // ASSERT
        var item = result.Items.Should().ContainSingle().Subject;
        item.SensorId.Should().Be(secondSensor.Id);
        item.Message.Should().Be("Kitchen smoke alert");
    }

    [Fact]
    public async Task Get_Should_RespectPagination()
    {
        // ARRANGE
        var (_, firstSensor, _) = await CreateAlertScenario();
        _context.SensorAlerts.AddRange(
            new SensorAlert { SensorId = firstSensor.Id, Message = "First", Type = AlertType.Error, Timestamp = DateTime.UtcNow.AddMinutes(-3) },
            new SensorAlert { SensorId = firstSensor.Id, Message = "Second", Type = AlertType.Error, Timestamp = DateTime.UtcNow.AddMinutes(-2) },
            new SensorAlert { SensorId = firstSensor.Id, Message = "Third", Type = AlertType.Error, Timestamp = DateTime.UtcNow.AddMinutes(-1) });
        await _context.SaveChangesAsync();

        var handler = new GetAlertsQueryHandler(_context);

        // ACT
        var result = await handler.Handle(new GetAlertsQuery { PageNumber = 2, PageSize = 1 }, CancellationToken.None);

        // ASSERT
        result.Items.Should().ContainSingle();
        result.PageNumber.Should().Be(2);
        result.TotalPages.Should().Be(3);
        result.TotalCount.Should().Be(3);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task Get_Should_ReturnEmptyListWhenThereAreNoAlerts()
    {
        // ARRANGE
        var handler = new GetAlertsQueryHandler(_context);

        // ACT
        var result = await handler.Handle(new GetAlertsQuery(), CancellationToken.None);

        // ASSERT
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public void Validator_Should_RejectInvalidPaginationAndType()
    {
        // ARRANGE
        var validator = new GetAlertsQueryValidator();

        // ACT
        var result = validator.TestValidate(new GetAlertsQuery
        {
            PageNumber = 0,
            PageSize = 101,
            Type = "InvalidType"
        });

        // ASSERT
        result.ShouldHaveValidationErrorFor(x => x.PageNumber);
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    private async Task<(Establishment Establishment, Sensor FirstSensor, Sensor SecondSensor)> CreateAlertScenario()
    {
        var establishment = await CreateEstablishment("Main Building");
        var firstLocation = new Location
        {
            EstablishmentId = establishment.Id,
            Name = "Living Room",
            Type = LocationType.Room
        };
        var secondLocation = new Location
        {
            EstablishmentId = establishment.Id,
            Name = "Kitchen",
            Type = LocationType.Room
        };
        var firstSensor = new Sensor
        {
            EstablishmentId = establishment.Id,
            Location = firstLocation,
            Name = "Door Sensor",
            DeviceId = "door-001",
            Type = SensorType.Door,
            Model = "D1",
            MinThreshold = 1,
            MaxThreshold = 10
        };
        var secondSensor = new Sensor
        {
            EstablishmentId = establishment.Id,
            Location = secondLocation,
            Name = "Smoke Sensor",
            DeviceId = "smoke-001",
            Type = SensorType.Smoke,
            Model = "S1",
            MinThreshold = 2,
            MaxThreshold = 20
        };

        _context.Sensors.AddRange(firstSensor, secondSensor);
        await _context.SaveChangesAsync();

        return (establishment, firstSensor, secondSensor);
    }
}
