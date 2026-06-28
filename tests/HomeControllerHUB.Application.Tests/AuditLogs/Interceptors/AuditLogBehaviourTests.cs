using FluentAssertions;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Infra.Interceptors;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HomeControllerHUB.Application.Tests.AuditLogs.Interceptors;

public class AuditLogBehaviourTests
{
    [Fact]
    public async Task Handle_AuditableCommandWithGuidResponse_RegistersEntityIdFromResponse()
    {
        var auditLogService = new Mock<IAuditLogService>();
        AuditLogEntry? capturedEntry = null;
        auditLogService
            .Setup(service => service.RegisterAsync(It.IsAny<AuditLogEntry>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLogEntry, CancellationToken>((entry, _) => capturedEntry = entry)
            .Returns(Task.CompletedTask);

        var behaviour = new AuditLogBehaviour<CreateAuditCommand, Guid>(
            auditLogService.Object,
            NullLogger<AuditLogBehaviour<CreateAuditCommand, Guid>>.Instance);
        var entityId = Guid.NewGuid();

        var response = await behaviour.Handle(new CreateAuditCommand(), () => Task.FromResult(entityId), CancellationToken.None);

        response.Should().Be(entityId);
        capturedEntry.Should().NotBeNull();
        capturedEntry!.EntityId.Should().Be(entityId.ToString());
        capturedEntry.Action.Should().Be("Create");
        capturedEntry.EntityName.Should().Be("Location");
    }

    [Fact]
    public async Task Handle_AuditableCommandWithUnitResponse_RegistersEntityIdFromCommand()
    {
        var command = new UpdateAuditCommand(Guid.NewGuid());
        var auditLogService = new Mock<IAuditLogService>();
        AuditLogEntry? capturedEntry = null;
        auditLogService
            .Setup(service => service.RegisterAsync(It.IsAny<AuditLogEntry>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLogEntry, CancellationToken>((entry, _) => capturedEntry = entry)
            .Returns(Task.CompletedTask);

        var behaviour = new AuditLogBehaviour<UpdateAuditCommand, Unit>(
            auditLogService.Object,
            NullLogger<AuditLogBehaviour<UpdateAuditCommand, Unit>>.Instance);

        var response = await behaviour.Handle(command, () => Task.FromResult(Unit.Value), CancellationToken.None);

        response.Should().Be(Unit.Value);
        capturedEntry.Should().NotBeNull();
        capturedEntry!.EntityId.Should().Be(command.Id.ToString());
        capturedEntry.Action.Should().Be("Update");
        capturedEntry.EntityName.Should().Be("Location");
    }

    [Fact]
    public async Task Handle_FailingCommand_DoesNotRegisterAuditLog()
    {
        var auditLogService = new Mock<IAuditLogService>();
        var behaviour = new AuditLogBehaviour<UpdateAuditCommand, Unit>(
            auditLogService.Object,
            NullLogger<AuditLogBehaviour<UpdateAuditCommand, Unit>>.Instance);

        var act = async () => await behaviour.Handle(
            new UpdateAuditCommand(Guid.NewGuid()),
            () => throw new InvalidOperationException("failed"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        auditLogService.Verify(service => service.RegisterAsync(It.IsAny<AuditLogEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private sealed record CreateAuditCommand : IAuditableCommand
    {
        public string AuditAction => "Create";
        public string AuditEntityName => "Location";
        public string? AuditEntityId => null;
        public string? AuditEntityDisplayName => null;
        public string? AuditDescription => null;
    }

    private sealed record UpdateAuditCommand(Guid Id) : IAuditableCommand
    {
        public string AuditAction => "Update";
        public string AuditEntityName => "Location";
        public string? AuditEntityId => Id.ToString();
        public string? AuditEntityDisplayName => null;
        public string? AuditDescription => null;
    }
}
