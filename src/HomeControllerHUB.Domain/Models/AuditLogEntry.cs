namespace HomeControllerHUB.Domain.Models;

public class AuditLogEntry
{
    public string Action { get; set; } = null!;
    public string EntityName { get; set; } = null!;
    public string? EntityId { get; set; }
    public string? EntityDisplayName { get; set; }
    public string? Description { get; set; }
    public object? Metadata { get; set; }
}
