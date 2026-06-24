using Asp.Versioning;
using HomeControllerHUB.Application.Alerts.Commands.AcknowledgeAlert;
using HomeControllerHUB.Application.Alerts.Queries.GetAlerts;
using HomeControllerHUB.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HomeControllerHUB.Api.Controllers;

/// <summary>
/// Manages global sensor alerts for administrative monitoring
/// </summary>
[ApiVersion(ApiConstants.ApiVersion1)]
public class AlertsController : ApiControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of alerts with filtering options
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<AlertListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedList<AlertListDto>>> Get([FromQuery] GetAlertsQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Marks an alert as acknowledged
    /// </summary>
    [HttpPatch("{id:guid}/acknowledge")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Acknowledge(Guid id)
    {
        await Mediator.Send(new AcknowledgeAlertCommand { Id = id });
        return NoContent();
    }
}
