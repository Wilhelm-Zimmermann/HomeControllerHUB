using HomeControllerHUB.Domain.Models;

namespace HomeControllerHUB.Domain.Interfaces;

public interface IAuditLogService
{
    Task RegisterAsync(AuditLogEntry entry, CancellationToken cancellationToken);
}
