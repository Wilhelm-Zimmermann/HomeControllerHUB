using HomeControllerHUB.Domain.Interfaces;

namespace HomeControllerHUB.Application.AuditLogs.Queries;

public class AuditLogDto : IPaginatedDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public Guid? EstablishmentId { get; set; }
    public string? Action { get; set; }
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public string? EntityDisplayName { get; set; }
    public string? Description { get; set; }
    public string? MetadataJson { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Created { get; set; }
}
