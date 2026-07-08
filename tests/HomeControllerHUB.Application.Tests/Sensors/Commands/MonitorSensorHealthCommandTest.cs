using FluentAssertions;
using HomeControllerHUB.Application.Sensors.Commands.MonitorSensorHealth;
using HomeControllerHUB.Application.Sensors.Commands.ProcessSensorReading;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Globalization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace HomeControllerHUB.Application.Tests.Sensors;

public class MonitorSensorHealthCommandTest : TestConfigs
{
    private readonly DateTime _now = new(2026, 7, 5, 12, 0, 0, DateTimeKind.Utc);
    private readonly Mock<IDateTime> _dateTimeMock;
    private readonly Mock<ISharedResource> _resourceMock;

    public MonitorSensorHealthCommandTest()
    {
        _dateTimeMock = new Mock<IDateTime>();
        _dateTimeMock.Setup(dateTime => dateTime.UtcNow).Returns(_now);

        _resourceMock = new Mock<ISharedResource>();
        _resourceMock.Setup(resource => resource.Message(It.IsAny<string>()))
            .Returns((string key) => key);
        _resourceMock.Setup(resource => resource.Message("SensorOffline"))
            .Returns("Sensor {0} has not communicated for more than {1} minutes.");
        _resourceMock.Setup(resource => resource.Message("SensorLowBattery"))
            .Returns("Sensor with low battery");
        _resourceMock.Setup(resource => resource.NotFoundMessage(It.IsAny<string>()))
            .Returns((string entity) => $"{entity} not found");
    }

    [Fact]
    public async Task Monitor_Should_CreateOfflineAlert_WhenActiveSensorHasNoLastCommunication()
    {
        var sensor = await CreateSensorAsync(
            lastCommunication: new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var handler = CreateHandler();

        var result = await handler.Handle(new MonitorSensorHealthCommand(), CancellationToken.None);

        result.SensorsChecked.Should().Be(1);
        result.OfflineAlertsCreated.Should().Be(1);
        _context.SensorAlerts.Should().ContainSingle(alert =>
            alert.SensorId == sensor.Id && alert.Type == AlertType.DeviceOffline && !alert.IsAcknowledged);
    }

    [Fact]
    public async Task Monitor_Should_CreateOfflineAlert_WhenActiveSensorCommunicationIsOld()
    {
        var sensor = await CreateSensorAsync(lastCommunication: _now.AddMinutes(-11));
        var handler = CreateHandler();

        var result = await handler.Handle(new MonitorSensorHealthCommand(), CancellationToken.None);

        result.OfflineAlertsCreated.Should().Be(1);
        _context.SensorAlerts.Should().ContainSingle(alert =>
            alert.SensorId == sensor.Id && alert.Type == AlertType.DeviceOffline);
    }

    [Fact]
    public async Task Monitor_Should_NotCreateOfflineAlert_WhenActiveSensorCommunicationIsRecent()
    {
        await CreateSensorAsync(lastCommunication: _now.AddMinutes(-9));
        var handler = CreateHandler();

        var result = await handler.Handle(new MonitorSensorHealthCommand(), CancellationToken.None);

        result.OfflineAlertsCreated.Should().Be(0);
        _context.SensorAlerts.Should().BeEmpty();
    }

    [Fact]
    public async Task Monitor_Should_NotCreateOfflineAlert_WhenSensorIsInactive()
    {
        await CreateSensorAsync(isActive: false, lastCommunication: _now.AddHours(-1));
        var handler = CreateHandler();

        var result = await handler.Handle(new MonitorSensorHealthCommand(), CancellationToken.None);

        result.SensorsChecked.Should().Be(0);
        result.OfflineAlertsCreated.Should().Be(0);
        _context.SensorAlerts.Should().BeEmpty();
    }

    [Fact]
    public async Task Monitor_Should_CreateLowBatteryAlert_WhenBatteryIsBelowThreshold()
    {
        var sensor = await CreateSensorAsync(lastCommunication: _now, batteryLevel: 19);
        var handler = CreateHandler();

        var result = await handler.Handle(new MonitorSensorHealthCommand(), CancellationToken.None);

        result.LowBatteryAlertsCreated.Should().Be(1);
        _context.SensorAlerts.Should().ContainSingle(alert =>
            alert.SensorId == sensor.Id && alert.Type == AlertType.BatteryLow && !alert.IsAcknowledged);
    }

    [Fact]
    public async Task Monitor_Should_NotCreateLowBatteryAlert_WhenBatteryIsNormal()
    {
        await CreateSensorAsync(lastCommunication: _now, batteryLevel: 20);
        var handler = CreateHandler();

        var result = await handler.Handle(new MonitorSensorHealthCommand(), CancellationToken.None);

        result.LowBatteryAlertsCreated.Should().Be(0);
        _context.SensorAlerts.Should().BeEmpty();
    }

    [Fact]
    public async Task Monitor_Should_SkipDuplicate_WhenPendingAlertExistsForSameSensorAndType()
    {
        var sensor = await CreateSensorAsync(lastCommunication: _now.AddHours(-1));
        _context.SensorAlerts.Add(new SensorAlert
        {
            SensorId = sensor.Id,
            Type = AlertType.DeviceOffline,
            Message = "Existing offline alert",
            Timestamp = _now.AddMinutes(-30),
            IsAcknowledged = false
        });
        await _context.SaveChangesAsync();
        var handler = CreateHandler();

        var result = await handler.Handle(new MonitorSensorHealthCommand(), CancellationToken.None);

        result.OfflineAlertsCreated.Should().Be(0);
        result.DuplicateAlertsSkipped.Should().Be(1);
        _context.SensorAlerts.Count(alert =>
            alert.SensorId == sensor.Id && alert.Type == AlertType.DeviceOffline).Should().Be(1);
    }

    [Fact]
    public async Task Monitor_Should_ReturnCorrectSummary_ForMultipleSensors()
    {
        await CreateSensorAsync(lastCommunication: _now.AddHours(-1), batteryLevel: 90);
        await CreateSensorAsync(lastCommunication: _now, batteryLevel: 10);
        await CreateSensorAsync(lastCommunication: _now.AddHours(-2), batteryLevel: 5);
        await CreateSensorAsync(isActive: false, lastCommunication: _now.AddHours(-3), batteryLevel: 1);
        var handler = CreateHandler();

        var result = await handler.Handle(new MonitorSensorHealthCommand(), CancellationToken.None);

        result.SensorsChecked.Should().Be(3);
        result.OfflineAlertsCreated.Should().Be(2);
        result.LowBatteryAlertsCreated.Should().Be(2);
        result.DuplicateAlertsSkipped.Should().Be(0);
        _context.SensorAlerts.Count().Should().Be(4);
    }

    [Fact]
    public async Task Monitor_Should_NotCreateAuditLog()
    {
        await CreateSensorAsync(lastCommunication: _now.AddHours(-1));
        var handler = CreateHandler();

        await handler.Handle(new MonitorSensorHealthCommand(), CancellationToken.None);

        _context.AuditLogs.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessReading_Should_KeepCreatingThresholdAlert_WhenValueExceedsMaxThreshold()
    {
        var sensor = await CreateSensorAsync(lastCommunication: _now, maxThreshold: 30);
        var handler = new ProcessSensorReadingCommandHandler(
            _context,
            _resourceMock.Object,
            NullLogger<ProcessSensorReadingCommandHandler>.Instance);

        var response = await handler.Handle(new ProcessSensorReadingCommand
        {
            SensorId = sensor.Id,
            DeviceId = sensor.DeviceId,
            MessageId = "threshold-message-1",
            Timestamp = _now,
            Value = 31,
            Unit = "C"
        }, CancellationToken.None);

        response.AlertCreated.Should().BeTrue();
        _context.SensorAlerts.Should().ContainSingle(alert =>
            alert.SensorId == sensor.Id && alert.Type == AlertType.ThresholdExceeded);
    }

    private MonitorSensorHealthCommandHandler CreateHandler(
        int offlineThresholdMinutes = 10,
        double lowBatteryThreshold = 20)
    {
        return new MonitorSensorHealthCommandHandler(
            _context,
            _dateTimeMock.Object,
            _resourceMock.Object,
            Options.Create(new SensorHealthMonitoringOptions
            {
                OfflineThresholdMinutes = offlineThresholdMinutes,
                LowBatteryThreshold = lowBatteryThreshold
            }));
    }

    private async Task<Sensor> CreateSensorAsync(
        bool isActive = true,
        DateTime? lastCommunication = null,
        double? batteryLevel = null,
        double? maxThreshold = null)
    {
        var establishment = await CreateEstablishment();
        var location = new Location
        {
            EstablishmentId = establishment.Id,
            Name = $"Location {Guid.NewGuid():N}"
        };

        var sensor = new Sensor
        {
            EstablishmentId = establishment.Id,
            Location = location,
            Name = $"Sensor {Guid.NewGuid():N}",
            DeviceId = $"DEVICE-{Guid.NewGuid():N}",
            Model = "ESP32",
            Type = SensorType.Temperature,
            IsActive = isActive,
            LastCommunication = lastCommunication ?? _now,
            BatteryLevel = batteryLevel,
            MaxThreshold = maxThreshold
        };

        _context.Sensors.Add(sensor);
        await _context.SaveChangesAsync();

        return sensor;
    }
}
