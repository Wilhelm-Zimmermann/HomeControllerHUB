using Asp.Versioning;
using HomeControllerHUB.Application.Privileges.Queries;
using HomeControllerHUB.Application.Privileges.Queries.PrivilegeSelector;
using HomeControllerHUB.Application.Profiles.Queries.GetProfileSelector;
using Microsoft.AspNetCore.Mvc;

namespace HomeControllerHUB.Api.Controllers;

[ApiVersion(ApiConstants.ApiVersion1)]
public class PrivilegeController : ApiControllerBase
{
    [HttpGet("list")]
    [ProducesResponseType(typeof(List<PrivilegeSelectorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<PrivilegeSelectorDto>>> GetSelector([FromQuery] PrivilegeSelectorQuery query, CancellationToken cancellationToken)
    {
        return await Mediator.Send(query, cancellationToken);
    }
}