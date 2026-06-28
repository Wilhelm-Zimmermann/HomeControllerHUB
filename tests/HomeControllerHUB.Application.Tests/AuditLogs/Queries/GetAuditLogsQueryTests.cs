using FluentAssertions;
using HomeControllerHUB.Application.AuditLogs.Queries.GetAuditLogs;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Infra.Interceptors;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HomeControllerHUB.Application.Tests.AuditLogs.Queries;

public class GetAuditLogsQueryTests
{
    [Fact]
    public async Task Handle_PaginatesAuditLogs()
    {
        await using var context = CreateContext();
        await SeedAuditLogs(context);
        var handler = new GetAuditLogsQueryHandler(context);

        var result = await handler.Handle(new GetAuditLogsQuery { PageNumber = 1, PageSize = 2 }, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(4);
        result.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task Handle_FiltersByAction()
    {
        await using var context = CreateContext();
        await SeedAuditLogs(context);
        var handler = new GetAuditLogsQueryHandler(context);

        var result = await handler.Handle(new GetAuditLogsQuery { Action = "Delete" }, CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].Action.Should().Be("Delete");
    }

    [Fact]
    public async Task Handle_FiltersByEntityName()
    {
        await using var context = CreateContext();
        await SeedAuditLogs(context);
        var handler = new GetAuditLogsQueryHandler(context);

        var result = await handler.Handle(new GetAuditLogsQuery { EntityName = "Sensor" }, CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].EntityName.Should().Be("Sensor");
    }

    [Fact]
    public async Task Handle_FiltersByUserId()
    {
        await using var context = CreateContext();
        var targetUserId = Guid.NewGuid();
        await SeedAuditLogs(context, targetUserId);
        var handler = new GetAuditLogsQueryHandler(context);

        var result = await handler.Handle(new GetAuditLogsQuery { UserId = targetUserId }, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(item => item.UserId == targetUserId);
    }

    [Fact]
    public async Task Handle_FiltersByCreatedPeriod()
    {
        await using var context = CreateContext();
        await SeedAuditLogs(context);
        var handler = new GetAuditLogsQueryHandler(context);
        var start = new DateTime(2026, 01, 02, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 01, 03, 23, 59, 59, DateTimeKind.Utc);

        var result = await handler.Handle(new GetAuditLogsQuery { CreatedStart = start, CreatedEnd = end }, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(item => item.Created >= start && item.Created <= end);
    }

    private static ApplicationDbContext CreateContext()
    {
        var currentUserService = new Mock<ICurrentUserService>();
        currentUserService.Setup(service => service.UserId).Returns(Guid.NewGuid());
        currentUserService.Setup(service => service.Login).Returns("test-user");
        currentUserService.Setup(service => service.EstablishmentId).Returns(Guid.NewGuid());

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(
            options,
            new NormalizedInterceptor(),
            new BaseEntityInterceptor(currentUserService.Object));
    }

    private static async Task SeedAuditLogs(ApplicationDbContext context, Guid? targetUserId = null)
    {
        var userId = targetUserId ?? Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var auditLogs = new List<AuditLog>
        {
            new()
            {
                UserId = userId,
                Action = "Create",
                EntityName = "Location",
                EntityId = Guid.NewGuid().ToString()
            },
            new()
            {
                UserId = userId,
                Action = "Update",
                EntityName = "Location",
                EntityId = Guid.NewGuid().ToString()
            },
            new()
            {
                UserId = otherUserId,
                Action = "Delete",
                EntityName = "Profile",
                EntityId = Guid.NewGuid().ToString()
            },
            new()
            {
                UserId = otherUserId,
                Action = "Acknowledge",
                EntityName = "Sensor",
                EntityId = Guid.NewGuid().ToString()
            }
        };

        context.AuditLogs.AddRange(auditLogs);
        await context.SaveChangesAsync();

        auditLogs[0].Created = new DateTime(2026, 01, 01, 12, 0, 0, DateTimeKind.Utc);
        auditLogs[1].Created = new DateTime(2026, 01, 02, 12, 0, 0, DateTimeKind.Utc);
        auditLogs[2].Created = new DateTime(2026, 01, 03, 12, 0, 0, DateTimeKind.Utc);
        auditLogs[3].Created = new DateTime(2026, 01, 04, 12, 0, 0, DateTimeKind.Utc);
        await context.SaveChangesAsync();
    }
}
