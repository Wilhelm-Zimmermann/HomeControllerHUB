namespace HomeControllerHUB.Domain.Interfaces;

public interface IAuditMetadataSanitizer
{
    object? Sanitize(object? metadata);
}
