using System.ComponentModel.DataAnnotations;
using HomeControllerHUB.Api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HomeControllerHUB.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]// api/v1/[controller]
[EnableRateLimiting(RateLimitingExtensions.AuthenticatedPolicy)]
/// <summary>
/// Base controller that all API controllers inherit from, providing common functionality
/// </summary>
public class ApiControllerBase : ControllerBase
{
    protected const string InvalidRequestMessage = "Invalid request, the Ids are different.";

    public bool UserIsAutheticated => HttpContext.User.Identity is not null ? HttpContext.User.Identity.IsAuthenticated : false;

    private ISender _mediator = null!;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected static void ThrowInvalidRequest(string message)
    {
        var error = new ValidationException();
        throw error;
    }
}