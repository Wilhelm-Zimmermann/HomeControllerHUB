using FluentAssertions;
using HomeControllerHUB.Application.Dashboard.Queries.GetDashboardSummary;
using HomeControllerHUB.Domain.Entities;

namespace HomeControllerHUB.Application.Tests.Dashboard.Queries;

public class GetDashboardSummaryQueryTest : TestConfigs
{
    [Fact]
    public async Task Get_Should_ReturnTotalSensorCount()
    {
        // ARRANGE
        await CreateDashboardScenario();
        var handler = new GetDashboardSummaryQueryHandler(_context);

        // ACT
        var result = await handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

        // ASSERT
        result.TotalSensors.Should().Be(3);
    }

    [Fact]
    public async Task Get_Should_SeparateActiveAndInactiveSensors()
    {
        // ARRANGE
        await CreateDashboardScenario();
        var handler = new GetDashboardSummaryQueryHandler(_context);

        // ACT
        var result = await handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

        // ASSERT
        result.ActiveSensors.Should().Be(2);
        result.InactiveSensors.Should().Be(1);
    }

    [Fact]
    public async Task Get_Should_CountLowBatterySensors()
    {
        // ARRANGE
        await CreateDashboardScenario();
        var handler = new GetDashboardSummaryQueryHandler(_context);

        // ACT
        var result = await handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

        // ASSERT
        result.LowBatterySensors.Should().Be(1);
    }

    [Fact]
    public async Task Get_Should_CountLocations()
    {
        // ARRANGE
        await CreateDashboardScenario();
        var handler = new GetDashboardSummaryQueryHandler(_context);

        // ACT
        var result = await handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

        // ASSERT
        result.TotalLocations.Should().Be(2);
    }

    [Fact]
    public async Task Get_Should_CountPendingAndAcknowledgedAlerts()
    {
        // ARRANGE
        await CreateDashboardScenario();
        var handler = new GetDashboardSummaryQueryHandler(_context);

        // ACT
        var result = await handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

        // ASSERT
        result.TotalAlerts.Should().Be(6);
        result.PendingAlerts.Should().Be(4);
        result.AcknowledgedAlerts.Should().Be(2);
    }

    [Fact]
    public async Task Get_Should_CountAlertsLast24Hours()
    {
        // ARRANGE
        await CreateDashboardScenario();
        var handler = new GetDashboardSummaryQueryHandler(_context);

        // ACT
        var result = await handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

        // ASSERT
        result.AlertsLast24Hours.Should().Be(5);
    }

    [Fact]
    public async Task Get_Should_CountReadingsLast24Hours()
    {
        // ARRANGE
        await CreateDashboardScenario();
        var handler = new GetDashboardSummaryQueryHandler(_context);

        // ACT
        var result = await handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

        // ASSERT
        result.ReadingsLast24Hours.Should().Be(2);
    }

    [Fact]
    public async Task Get_Should_ReturnLastFiveRecentAlerts()
    {
        // ARRANGE
        var (_, _, alerts) = await CreateDashboardScenario();
        var handler = new GetDashboardSummaryQueryHandler(_context);

        // ACT
        var result = await handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

        // ASSERT
        result.RecentAlerts.Should().HaveCount(5);
        result.RecentAlerts.Select(alert => alert.Id).Should().Equal(
            alerts.OrderByDescending(alert => alert.Created).Take(5).Select(alert => alert.Id));

        var recentAlert = result.RecentAlerts.First();
        recentAlert.SensorName.Should().NotBeNullOrWhiteSpace();
        recentAlert.SensorDeviceId.Should().NotBeNullOrWhiteSpace();
        recentAlert.LocationName.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Get_Should_FilterByEstablishment()
    {
        // ARRANGE
        var (establishment, _, _) = await CreateDashboardScenario();
        await CreateDashboardScenario("Secondary Building");
        var handler = new GetDashboardSummaryQueryHandler(_context);

        // ACT
        var result = await handler.Handle(
            new GetDashboardSummaryQuery { EstablishmentId = establishment.Id },
            CancellationToken.None);

        // ASSERT
        result.TotalSensors.Should().Be(3);
        result.TotalLocations.Should().Be(2);
        result.TotalAlerts.Should().Be(6);
        result.ReadingsLast24Hours.Should().Be(2);
    }

    [Fact]
    public async Task Get_Should_ReturnZerosAndEmptyRecentAlertsWhenThereIsNoData()
    {
        // ARRANGE
        var handler = new GetDashboardSummaryQueryHandler(_context);

        // ACT
        var result = await handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

        // ASSERT
        result.TotalSensors.Should().Be(0);
        result.ActiveSensors.Should().Be(0);
        result.InactiveSensors.Should().Be(0);
        result.LowBatterySensors.Should().Be(0);
        result.TotalLocations.Should().Be(0);
        result.TotalAlerts.Should().Be(0);
        result.PendingAlerts.Should().Be(0);
        result.AcknowledgedAlerts.Should().Be(0);
        result.AlertsLast24Hours.Should().Be(0);
        result.ReadingsLast24Hours.Should().Be(0);
        result.RecentAlerts.Should().BeEmpty();
    }

    private async Task<(Establishment Establishment, List<Sensor> Sensors, List<SensorAlert> Alerts)> CreateDashboardScenario(
        string establishmentName = "Main Building")
    {
        var establishment = await CreateEstablishment(establishmentName);
        var livingRoom = new Location
        {
            EstablishmentId = establishment.Id,
            Name = $"{establishmentName} Living Room",
            Type = LocationType.Room
        };
        var kitchen = new Location
        {
            EstablishmentId = establishment.Id,
            Name = $"{establishmentName} Kitchen",
            Type = LocationType.Room
        };

        var sensors = new List<Sensor>
        {
            new()
            {
                EstablishmentId = establishment.Id,
                Location = livingRoom,
                Name = $"{establishmentName} Door Sensor",
                DeviceId = $"{establishmentName}-door-001",
                Type = SensorType.Door,
                Model = "D1",
                IsActive = true,
                BatteryLevel = 80
            },
            new()
            {
                EstablishmentId = establishment.Id,
                Location = kitchen,
                Name = $"{establishmentName} Smoke Sensor",
                DeviceId = $"{establishmentName}-smoke-001",
                Type = SensorType.Smoke,
                Model = "S1",
                IsActive = true,
                BatteryLevel = 10
            },
            new()
            {
                EstablishmentId = establishment.Id,
                Location = livingRoom,
                Name = $"{establishmentName} Motion Sensor",
                DeviceId = $"{establishmentName}-motion-001",
                Type = SensorType.Motion,
                Model = "M1",
                IsActive = false,
                BatteryLevel = null
            }
        };

        _context.Sensors.AddRange(sensors);
        await _context.SaveChangesAsync();

        var now = DateTime.UtcNow;
        var alerts = new List<SensorAlert>
        {
            CreateAlert(sensors[0], AlertType.DeviceOffline, "Offline", false, now.AddMinutes(-1)),
            CreateAlert(sensors[1], AlertType.BatteryLow, "Battery low", false, now.AddMinutes(-2)),
            CreateAlert(sensors[2], AlertType.Error, "Error", true, now.AddMinutes(-3)),
            CreateAlert(sensors[0], AlertType.ThresholdExceeded, "High value", false, now.AddMinutes(-4)),
            CreateAlert(sensors[1], AlertType.ThresholdBelowMinimum, "Low value", true, now.AddMinutes(-5)),
            CreateAlert(sensors[2], AlertType.Error, "Old error", false, now.AddHours(-25))
        };

        _context.SensorAlerts.AddRange(alerts);
        _context.SensorReadings.AddRange(
            new SensorReading { SensorId = sensors[0].Id, Timestamp = now.AddMinutes(-10), Value = 20 },
            new SensorReading { SensorId = sensors[1].Id, Timestamp = now.AddHours(-2), Value = 50 },
            new SensorReading { SensorId = sensors[2].Id, Timestamp = now.AddHours(-26), Value = 1 });
        await _context.SaveChangesAsync();

        foreach (var alert in alerts)
        {
            alert.Created = alert.Timestamp;
        }

        await _context.SaveChangesAsync();

        return (establishment, sensors, alerts);
    }

    private static SensorAlert CreateAlert(
        Sensor sensor,
        AlertType type,
        string message,
        bool isAcknowledged,
        DateTime timestamp)
    {
        return new SensorAlert
        {
            SensorId = sensor.Id,
            Type = type,
            Message = message,
            IsAcknowledged = isAcknowledged,
            Timestamp = timestamp
        };
    }
}
