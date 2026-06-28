using HomeControllerHUB.Domain.Models;
using Newtonsoft.Json;

namespace HomeControllerHUB.Api.Middlewares;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception for {Method} {Path} with correlation {CorrelationId}",
                context.Request.Method,
                context.Request.Path.Value,
                GetCorrelationId(context));

            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        string result;
        if (exception is AppError error)
        {
            context.Response.StatusCode = error.StatusCode;
            result = JsonConvert.SerializeObject(new { Error = error.Message, Description = error.Description });
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(result);
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        result = JsonConvert.SerializeObject(new { error = exception.Message });
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(result);
    }

    private static string? GetCorrelationId(HttpContext context)
    {
        return context.Items.TryGetValue(CorrelationIdMiddleware.ItemKey, out var correlationId)
            ? correlationId?.ToString()
            : context.Response.Headers[CorrelationIdMiddleware.HeaderName].FirstOrDefault();
    }
}
