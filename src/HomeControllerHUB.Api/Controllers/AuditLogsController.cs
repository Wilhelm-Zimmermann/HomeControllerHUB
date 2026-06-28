using Asp.Versioning;
using HomeControllerHUB.Application.AuditLogs.Queries;
using HomeControllerHUB.Application.AuditLogs.Queries.GetAuditLogs;
using HomeControllerHUB.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace HomeControllerHUB.Api.Controllers;

[ApiVersion(ApiConstants.ApiVersion1)]
public class AuditLogsController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedList<AuditLogDto>>> Get([FromQuery] GetAuditLogsQuery query, CancellationToken cancellationToken)
    {
        return await Mediator.Send(query, cancellationToken);
    }
}
