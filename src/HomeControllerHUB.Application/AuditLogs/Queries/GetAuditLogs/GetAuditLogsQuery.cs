using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Shared.Common;
using HomeControllerHUB.Shared.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Application.AuditLogs.Queries.GetAuditLogs;

[Authorize(Domain = DomainNames.AuditLog, Action = SecurityActionType.Read)]
public record GetAuditLogsQuery : PaginatedRequest<AuditLogDto>
{
    public Guid? UserId { get; init; }
    public Guid? EstablishmentId { get; init; }
    public string? EntityName { get; init; }
    public string? EntityId { get; init; }
    public string? Action { get; init; }
    public DateTime? CreatedStart { get; init; }
    public DateTime? CreatedEnd { get; init; }
}

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PaginatedList<AuditLogDto>>
{
    private readonly ApplicationDbContext _context;

    public GetAuditLogsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AuditLogs.AsNoTracking();

        if (request.UserId.HasValue)
        {
            query = query.Where(auditLog => auditLog.UserId == request.UserId.Value);
        }

        if (request.EstablishmentId.HasValue)
        {
            query = query.Where(auditLog => auditLog.EstablishmentId == request.EstablishmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.EntityName))
        {
            query = query.Where(auditLog => auditLog.EntityName == request.EntityName);
        }

        if (!string.IsNullOrWhiteSpace(request.EntityId))
        {
            query = query.Where(auditLog => auditLog.EntityId == request.EntityId);
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            query = query.Where(auditLog => auditLog.Action == request.Action);
        }

        if (request.CreatedStart.HasValue)
        {
            query = query.Where(auditLog => auditLog.Created >= request.CreatedStart.Value);
        }

        if (request.CreatedEnd.HasValue)
        {
            query = query.Where(auditLog => auditLog.Created <= request.CreatedEnd.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchBy))
        {
            var searchBy = request.SearchBy.ToLower();
            query = query.Where(auditLog =>
                (auditLog.UserName != null && auditLog.UserName.ToLower().Contains(searchBy)) ||
                (auditLog.Action != null && auditLog.Action.ToLower().Contains(searchBy)) ||
                (auditLog.EntityName != null && auditLog.EntityName.ToLower().Contains(searchBy)) ||
                (auditLog.EntityId != null && auditLog.EntityId.ToLower().Contains(searchBy)) ||
                (auditLog.EntityDisplayName != null && auditLog.EntityDisplayName.ToLower().Contains(searchBy)) ||
                (auditLog.Description != null && auditLog.Description.ToLower().Contains(searchBy)));
        }

        var projectedQuery = query.Select(auditLog => new AuditLogDto
        {
            Id = auditLog.Id,
            UserId = auditLog.UserId,
            UserName = auditLog.UserName,
            EstablishmentId = auditLog.EstablishmentId,
            Action = auditLog.Action,
            EntityName = auditLog.EntityName,
            EntityId = auditLog.EntityId,
            EntityDisplayName = auditLog.EntityDisplayName,
            Description = auditLog.Description,
            MetadataJson = auditLog.MetadataJson,
            IpAddress = auditLog.IpAddress,
            UserAgent = auditLog.UserAgent,
            Created = auditLog.Created
        });

        if (string.IsNullOrWhiteSpace(request.OrderBy))
        {
            projectedQuery = projectedQuery.OrderByDescending(auditLog => auditLog.Created);
        }

        return await projectedQuery.PaginateAsync(request, cancellationToken);
    }
}
