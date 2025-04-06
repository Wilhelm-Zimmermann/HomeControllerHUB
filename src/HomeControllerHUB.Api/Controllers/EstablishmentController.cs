using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using HomeControllerHUB.Application.Establishments.Commands.CreateEstablishment;
using HomeControllerHUB.Application.Establishments.Commands.DeleteEstablishment;
using HomeControllerHUB.Application.Establishments.Commands.UpdateEstablishment;
using HomeControllerHUB.Application.Establishments.Queries;
using HomeControllerHUB.Application.Establishments.Queries.GetAllEstablishmentPaginated;
using HomeControllerHUB.Application.Establishments.Queries.GetEstablishmentById;
using HomeControllerHUB.Application.Establishments.Queries.GetEstablishmentSelector;
using HomeControllerHUB.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using EstablishmentSelectorDto = HomeControllerHUB.Application.Establishments.Queries.GetEstablishmentSelector.EstablishmentSelectorDto;

namespace HomeControllerHUB.Api.Controllers;

[ApiVersion(ApiConstants.ApiVersion1)]
/// <summary>
/// Manages establishments (e.g., buildings, facilities, campuses) in the system
/// </summary>
public class EstablishmentController : ApiControllerBase
{
    /// <summary>
    /// Creates a new establishment
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BaseEntityResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BaseEntityResponse>> Create([FromBody] CreateEstablishmentCommand command, CancellationToken cancellationToken)
    {
        return await Mediator.Send(command, cancellationToken);
    }
    
    /// <summary>
    /// Updates an existing establishment
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update([Required] Guid id, [FromBody] UpdateEstablishmentCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            ThrowInvalidRequest(InvalidRequestMessage);
        
        await Mediator.Send(command, cancellationToken);
        return Ok();
    }
    
    /// <summary>
    /// Deletes an establishment
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete([Required] Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteEstablishmentCommand(id), cancellationToken);
        return NoContent();
    }
    
    /// <summary>
    /// Retrieves a paginated list of establishments
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<EstablishmentWithPaginationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedList<EstablishmentWithPaginationDto>>> GetALlWithPagination([FromQuery] GetAllEstablishmentPaginatedQuery query, CancellationToken cancellationToken)
    {
        return await Mediator.Send(query, cancellationToken);
    }
    
    /// <summary>
    /// Retrieves a list of establishments for dropdown selection
    /// </summary>
    [HttpGet("list")]
    [ProducesResponseType(typeof(List<EstablishmentSelectorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<EstablishmentSelectorDto>>> GetSelector(CancellationToken cancellationToken)
    {
        return await Mediator.Send(new GetEstablishmentSelectorQuery(), cancellationToken);
    }
        
    /// <summary>
    /// Retrieves a specific establishment by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EstablishmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EstablishmentDto>> GetSelector([Required] Guid id, CancellationToken cancellationToken)
    {
        return await Mediator.Send(new GetEstablishmentByIdQuery(id), cancellationToken);
    }
}