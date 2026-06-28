using System.Diagnostics;

namespace HomeControllerHUB.Api.Middlewares;

public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";
    public const string ItemKey = "CorrelationId";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        context.Items[ItemKey] = correlationId;
        context.TraceIdentifier = correlationId;
        context.Response.Headers[HeaderName] = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            [ItemKey] = correlationId
        });

        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();

        _logger.LogInformation(
            "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms for user {UserId} from {RemoteIpAddress} with correlation {CorrelationId}",
            context.Request.Method,
            context.Request.Path.Value,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            context.User?.Identity?.Name,
            context.Connection.RemoteIpAddress?.ToString(),
            correlationId);
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Items.TryGetValue(ItemKey, out var existingCorrelationId) &&
            existingCorrelationId is string existingCorrelationIdValue &&
            !string.IsNullOrWhiteSpace(existingCorrelationIdValue))
        {
            return existingCorrelationIdValue;
        }

        var headerValue = context.Request.Headers[HeaderName].FirstOrDefault();

        return string.IsNullOrWhiteSpace(headerValue)
            ? Guid.NewGuid().ToString()
            : headerValue;
    }
}
