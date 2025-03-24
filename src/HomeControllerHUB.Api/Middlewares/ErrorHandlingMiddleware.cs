using HomeControllerHUB.Domain.Models;
using Newtonsoft.Json;

namespace HomeControllerHUB.Api.Middlewares;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
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
}