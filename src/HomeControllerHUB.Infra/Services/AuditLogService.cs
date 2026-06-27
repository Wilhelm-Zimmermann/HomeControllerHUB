using System.Text.Json;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Infra.DatabaseContext;
using Microsoft.AspNetCore.Http;

namespace HomeControllerHUB.Infra.Services;

public class AuditLogService : IAuditLogService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditMetadataSanitizer _metadataSanitizer;

    public AuditLogService(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor,
        IAuditMetadataSanitizer metadataSanitizer)
    {
        _context = context;
        _currentUserService = currentUserService;
        _httpContextAccessor = httpContextAccessor;
        _metadataSanitizer = metadataSanitizer;
    }

    public async Task RegisterAsync(AuditLogEntry entry, CancellationToken cancellationToken)
    {
        var sanitizedMetadata = _metadataSanitizer.Sanitize(entry.Metadata);
        var httpContext = _httpContextAccessor.HttpContext;

        var auditLog = new AuditLog
        {
            UserId = _currentUserService.UserId,
            UserName = _currentUserService.Login,
            EstablishmentId = TryGetEstablishmentId(),
            Action = entry.Action,
            EntityName = entry.EntityName,
            EntityId = entry.EntityId,
            EntityDisplayName = entry.EntityDisplayName,
            Description = entry.Description,
            MetadataJson = sanitizedMetadata is null ? null : JsonSerializer.Serialize(sanitizedMetadata, SerializerOptions),
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString()
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private Guid? TryGetEstablishmentId()
    {
        try
        {
            return _currentUserService.EstablishmentId;
        }
        catch
        {
            return null;
        }
    }
}
