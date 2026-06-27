namespace HomeControllerHUB.Domain.Interfaces;

public interface IAuditableCommand
{
    string AuditAction { get; }
    string AuditEntityName { get; }
    string? AuditEntityId { get; }
    string? AuditEntityDisplayName { get; }
    string? AuditDescription { get; }
    object? AuditMetadata => this;
}
