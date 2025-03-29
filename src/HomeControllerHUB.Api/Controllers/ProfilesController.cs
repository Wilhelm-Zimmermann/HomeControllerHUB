using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using HomeControllerHUB.Application.Profiles.Queries;
using HomeControllerHUB.Application.Profiles.Queries.GetAllProfilePaginated;
using HomeControllerHUB.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeControllerHUB.Api.Controllers;

[ApiVersion(ApiConstants.ApiVersion1)]
public class ProfilesController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<GetProfilePaginatedDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedList<GetProfilePaginatedDto>>> GetALlWithPagination([FromQuery] GetAllProfilePaginatedQuery query, CancellationToken cancellationToken)
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return await Mediator.Send(query, cancellationToken);
    }
}