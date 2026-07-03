using System.Security.Claims;
using System.Threading.RateLimiting;
using HomeControllerHUB.Api.Middlewares;
using Microsoft.AspNetCore.RateLimiting;

namespace HomeControllerHUB.Api.Extensions;

public static class RateLimitingExtensions
{
    public const string AuthenticatedPolicy = nameof(AuthenticatedPolicy);
    public const string AuthPolicy = nameof(AuthPolicy);
    public const string SensitivePolicy = nameof(SensitivePolicy);
    public const string SensorIngestionPolicy = nameof(SensorIngestionPolicy);

    private const int AuthenticatedPermitLimit = 100;
    private const int AuthPermitLimit = 10;
    private const int SensitivePermitLimit = 20;
    private const int SensorIngestionPermitLimit = 60;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    public static IServiceCollection AddHomeControllerHubRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = OnRejectedAsync;

            options.AddPolicy(AuthenticatedPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetAuthenticatedPartitionKey(httpContext),
                    _ => CreateFixedWindowOptions(AuthenticatedPermitLimit)));

            options.AddPolicy(AuthPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetIpPartitionKey(httpContext),
                    _ => CreateFixedWindowOptions(AuthPermitLimit)));

            options.AddPolicy(SensitivePolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetAuthenticatedPartitionKey(httpContext),
                    _ => CreateFixedWindowOptions(SensitivePermitLimit)));

            options.AddPolicy(SensorIngestionPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetIpPartitionKey(httpContext),
                    _ => CreateFixedWindowOptions(SensorIngestionPermitLimit)));
        });

        return services;
    }

    private static FixedWindowRateLimiterOptions CreateFixedWindowOptions(int permitLimit)
    {
        return new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = Window,
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            AutoReplenishment = true
        };
    }

    private static async ValueTask OnRejectedAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var httpContext = context.HttpContext;
        var correlationId = GetCorrelationId(httpContext);
        var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
            ? retryAfterValue
            : (TimeSpan?)null;

        if (retryAfter is not null)
        {
            httpContext.Response.Headers.RetryAfter = Math.Ceiling(retryAfter.Value.TotalSeconds).ToString();
        }

        var logger = httpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("HomeControllerHUB.Api.RateLimiting");

        logger.LogWarning(
            "Rate limit exceeded for {Method} {Path} using policy {Policy} from {RemoteIpAddress} for user {UserId} with correlation {CorrelationId}",
            httpContext.Request.Method,
            httpContext.Request.Path.Value,
            GetPolicyName(httpContext),
            httpContext.Connection.RemoteIpAddress?.ToString(),
            GetUserId(httpContext),
            correlationId);

        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(new
        {
            title = "Too many requests",
            message = "Voce fez muitas requisicoes em pouco tempo. Tente novamente em alguns instantes.",
            statusCode = StatusCodes.Status429TooManyRequests,
            correlationId
        }, cancellationToken);
    }

    private static string GetAuthenticatedPartitionKey(HttpContext httpContext)
    {
        var userId = GetUserId(httpContext);
        return string.IsNullOrWhiteSpace(userId)
            ? GetIpPartitionKey(httpContext)
            : $"user:{userId}";
    }

    private static string GetIpPartitionKey(HttpContext httpContext)
    {
        return $"ip:{httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
    }

    private static string? GetUserId(HttpContext httpContext)
    {
        return httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? httpContext.User.FindFirstValue("sub")
            ?? httpContext.User.Identity?.Name;
    }

    private static string GetCorrelationId(HttpContext httpContext)
    {
        if (httpContext.Items.TryGetValue(CorrelationIdMiddleware.ItemKey, out var correlationId) &&
            correlationId is not null)
        {
            return correlationId.ToString()!;
        }

        return httpContext.Response.Headers[CorrelationIdMiddleware.HeaderName].FirstOrDefault()
            ?? httpContext.TraceIdentifier;
    }

    private static string GetPolicyName(HttpContext httpContext)
    {
        return httpContext.GetEndpoint()
                   ?.Metadata
                   .GetMetadata<EnableRateLimitingAttribute>()
                   ?.PolicyName
               ?? "Unknown";
    }
}
