using System.Reflection;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HomeControllerHUB.Infra.Interceptors;

public class AuditLogBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuditLogBehaviour<TRequest, TResponse>> _logger;

    public AuditLogBehaviour(
        IAuditLogService auditLogService,
        ILogger<AuditLogBehaviour<TRequest, TResponse>> logger)
    {
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        if (request is not IAuditableCommand auditableCommand)
        {
            return response;
        }

        try
        {
            await _auditLogService.RegisterAsync(new AuditLogEntry
            {
                Action = auditableCommand.AuditAction,
                EntityName = auditableCommand.AuditEntityName,
                EntityId = auditableCommand.AuditEntityId ?? ExtractEntityId(response),
                EntityDisplayName = auditableCommand.AuditEntityDisplayName,
                Description = auditableCommand.AuditDescription,
                Metadata = new { request = auditableCommand.AuditMetadata ?? request }
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register audit log for {RequestName}", typeof(TRequest).Name);
        }

        return response;
    }

    private static string? ExtractEntityId(TResponse response)
    {
        if (response is null)
        {
            return null;
        }

        if (response is Guid guid)
        {
            return guid == Guid.Empty ? null : guid.ToString();
        }

        var idProperty = response.GetType().GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
        var id = idProperty?.GetValue(response);

        return id switch
        {
            Guid value when value != Guid.Empty => value.ToString(),
            string value when !string.IsNullOrWhiteSpace(value) => value,
            _ => id?.ToString()
        };
    }
}
