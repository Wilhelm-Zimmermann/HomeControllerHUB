using FluentAssertions;
using FluentValidation.TestHelper;
using HomeControllerHUB.Application.Alerts.Commands.AcknowledgeAlert;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HomeControllerHUB.Application.Tests.Alerts.Commands;

public class AcknowledgeAlertCommandTest : TestConfigs
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ISharedResource> _resourceMock;

    public AcknowledgeAlertCommandTest()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _resourceMock = new Mock<ISharedResource>();
    }

    [Fact]
    public async Task Acknowledge_Should_UpdatePendingAlert()
    {
        // ARRANGE
        var (alert, user) = await CreatePendingAlertScenario();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(user.Id);
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new AcknowledgeAlertCommand { Id = alert.Id }, CancellationToken.None);

        // ASSERT
        _context.ChangeTracker.Clear();
        var alertInDb = await _context.SensorAlerts.FindAsync(alert.Id);
        alertInDb.Should().NotBeNull();
        alertInDb!.IsAcknowledged.Should().BeTrue();
        alertInDb.AcknowledgedAt.Should().NotBeNull();
        alertInDb.AcknowledgedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        alertInDb.AcknowledgedById.Should().Be(user.Id);
    }

    [Fact]
    public async Task Acknowledge_Should_PersistChangeInDatabase()
    {
        // ARRANGE
        var (alert, user) = await CreatePendingAlertScenario();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(user.Id);
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new AcknowledgeAlertCommand { Id = alert.Id }, CancellationToken.None);

        // ASSERT
        _context.ChangeTracker.Clear();
        var persistedAlert = await _context.SensorAlerts
            .AsNoTracking()
            .FirstAsync(x => x.Id == alert.Id);

        persistedAlert.IsAcknowledged.Should().BeTrue();
        persistedAlert.AcknowledgedAt.Should().NotBeNull();
        persistedAlert.AcknowledgedById.Should().Be(user.Id);
    }

    [Fact]
    public async Task Acknowledge_Should_BeIdempotent_WhenAlertIsAlreadyAcknowledged()
    {
        // ARRANGE
        var acknowledgedAt = DateTime.UtcNow.AddHours(-1);
        var (alert, user) = await CreatePendingAlertScenario();
        alert.IsAcknowledged = true;
        alert.AcknowledgedAt = acknowledgedAt;
        alert.AcknowledgedById = null;
        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(user.Id);
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new AcknowledgeAlertCommand { Id = alert.Id }, CancellationToken.None);

        // ASSERT
        _context.ChangeTracker.Clear();
        var alertInDb = await _context.SensorAlerts.FindAsync(alert.Id);
        alertInDb.Should().NotBeNull();
        alertInDb!.IsAcknowledged.Should().BeTrue();
        alertInDb.AcknowledgedAt.Should().BeCloseTo(acknowledgedAt, TimeSpan.FromMilliseconds(1));
        alertInDb.AcknowledgedById.Should().BeNull();
    }

    [Fact]
    public async Task Acknowledge_Should_ThrowAppError_WhenAlertDoesNotExist()
    {
        // ARRANGE
        var handler = CreateHandler();

        // ACT
        Func<Task> act = async () =>
            await handler.Handle(
                new AcknowledgeAlertCommand { Id = Guid.NewGuid() },
                CancellationToken.None);

        // ASSERT
        await act.Should()
            .ThrowAsync<AppError>()
            .Where(ex => ex.StatusCode == StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Acknowledge_Should_NotChangeOtherAlerts()
    {
        // ARRANGE
        var (alert, user) = await CreatePendingAlertScenario();
        var otherAlert = new SensorAlert
        {
            SensorId = alert.SensorId,
            Message = "Other alert",
            Type = AlertType.Error,
            IsAcknowledged = false,
            Timestamp = DateTime.UtcNow
        };
        _context.SensorAlerts.Add(otherAlert);
        await _context.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(user.Id);
        var handler = CreateHandler();

        // ACT
        await handler.Handle(new AcknowledgeAlertCommand { Id = alert.Id }, CancellationToken.None);

        // ASSERT
        _context.ChangeTracker.Clear();
        var otherAlertInDb = await _context.SensorAlerts.FindAsync(otherAlert.Id);
        otherAlertInDb.Should().NotBeNull();
        otherAlertInDb!.IsAcknowledged.Should().BeFalse();
        otherAlertInDb.AcknowledgedAt.Should().BeNull();
        otherAlertInDb.AcknowledgedById.Should().BeNull();
    }

    [Fact]
    public void Validation_Should_Fail_WhenIdIsEmpty()
    {
        // ARRANGE
        var validator = new AcknowledgeAlertCommandValidator();

        // ACT
        var result = validator.TestValidate(new AcknowledgeAlertCommand { Id = Guid.Empty });

        // ASSERT
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    private AcknowledgeAlertCommandHandler CreateHandler()
    {
        return new AcknowledgeAlertCommandHandler(
            _context,
            _currentUserServiceMock.Object,
            _resourceMock.Object);
    }

    private async Task<(SensorAlert Alert, ApplicationUser User)> CreatePendingAlertScenario()
    {
        var establishment = await CreateEstablishment();
        var location = new Location
        {
            EstablishmentId = establishment.Id,
            Name = "Alert Room",
            Type = LocationType.Room
        };
        var sensor = new Sensor
        {
            EstablishmentId = establishment.Id,
            Location = location,
            Name = "Temperature Sensor",
            DeviceId = "temp-001",
            Type = SensorType.Temperature,
            Model = "T1"
        };
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            EstablishmentId = establishment.Id,
            Name = "Operator",
            NormalizedName = "OPERATOR",
            UserName = "operator",
            NormalizedUserName = "OPERATOR",
            Login = "operator",
            Email = "operator@example.com",
            Code = "operator",
            PasswordHash = "hashed-password",
            Enable = true
        };
        var alert = new SensorAlert
        {
            Sensor = sensor,
            Message = "Temperature threshold exceeded",
            Type = AlertType.ThresholdExceeded,
            IsAcknowledged = false,
            Timestamp = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.SensorAlerts.Add(alert);
        await _context.SaveChangesAsync();

        return (alert, user);
    }
}
