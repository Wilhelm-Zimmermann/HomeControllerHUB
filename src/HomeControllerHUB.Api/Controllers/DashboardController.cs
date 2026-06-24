using Asp.Versioning;
using HomeControllerHUB.Application.Dashboard.Queries.GetDashboardSummary;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HomeControllerHUB.Api.Controllers;

/// <summary>
/// Provides administrative dashboard summaries
/// </summary>
[ApiVersion(ApiConstants.ApiVersion1)]
public class DashboardController : ApiControllerBase
{
    /// <summary>
    /// Retrieves aggregated data for the dashboard
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DashboardSummaryDto>> Summary([FromQuery] GetDashboardSummaryQuery query, CancellationToken cancellationToken)
    {
        return await Mediator.Send(query, cancellationToken);
    }
}
