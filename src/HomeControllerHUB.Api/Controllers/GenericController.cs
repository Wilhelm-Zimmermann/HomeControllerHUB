using Asp.Versioning;
using HomeControllerHUB.Application.Generics.Queries;
using HomeControllerHUB.Application.Generics.Queries.GenericSelector;
using Microsoft.AspNetCore.Mvc;

namespace HomeControllerHUB.Api.Controllers;

[ApiVersion(ApiConstants.ApiVersion1)]
/// <summary>
/// Provides generic data endpoints for system-wide lookup values and enumerations
/// </summary>
public class GenericController : ApiControllerBase
{
    /// <summary>
    /// Retrieves a list of generic data items for dropdowns and selections
    /// </summary>
    [HttpGet("list")]
    [ProducesResponseType(typeof(List<GenericDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<GenericDto>>> GetSelector([FromQuery] GenericSelectorQuery query, CancellationToken cancellationToken)
    {
        return await Mediator.Send(query, cancellationToken);
    }
}