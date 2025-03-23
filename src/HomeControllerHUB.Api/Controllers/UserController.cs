using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using HomeControllerHUB.Application.Users.Commands.AccessTokenUser;
using HomeControllerHUB.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeControllerHUB.Api.Controllers;

[ApiVersion(ApiConstants.ApiVersion1)]
public class UserController : ApiControllerBase
{
    [HttpPost("[action]")]
    [ProducesResponseType(typeof(AccessTokenEntry), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    public async Task<ActionResult<AccessTokenEntry>> Token([Required, FromBody] AccessTokenUserCommand command, CancellationToken cancellationToken)
    {
        return await Mediator.Send(command, cancellationToken);
    }
}