using HomeControllerHUB.Application.Locations.Commands.CreateLocation;
using HomeControllerHUB.Application.Locations.Commands.DeleteLocation;
using HomeControllerHUB.Application.Locations.Commands.UpdateLocation;
using HomeControllerHUB.Application.Locations.Queries;
using HomeControllerHUB.Application.Locations.Queries.GetLocation;
using HomeControllerHUB.Application.Locations.Queries.GetLocationHierarchy;
using HomeControllerHUB.Application.Locations.Queries.GetLocationList;
using HomeControllerHUB.Application.Locations.Queries.GetLocations;
using HomeControllerHUB.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Asp.Versioning;

namespace HomeControllerHUB.Api.Controllers;

[ApiVersion(ApiConstants.ApiVersion1)]
/// <summary>
/// Manages locations within establishments (e.g., rooms, floors, zones)
/// </summary>
public class LocationsController : ApiControllerBase
{
    /// <summary>
    /// Creates a new location
    /// </summary>
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(BaseEntityResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Guid>> Create(CreateLocationCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(Get), new { id = result }, new BaseEntityResponse { Id = result });
    }
    
    /// <summary>
    /// Updates an existing location
    /// </summary>
    [HttpPut]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(UpdateLocationCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }
    
    /// <summary>
    /// Deletes a location
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete([FromBody] DeleteLocationCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }
    
    /// <summary>
    /// Retrieves a specific location by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LocationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LocationDto>> Get(Guid id)
    {
        return await Mediator.Send(new GetLocationQuery { Id = id });
    }
    
    /// <summary>
    /// Retrieves a paginated list of locations with filtering options
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<LocationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedList<LocationDto>>> Get([FromQuery] GetLocationsQuery query)
    {
        return await Mediator.Send(query);
    }
    
    /// <summary>
    /// Retrieves a complete list of locations for dropdown selection
    /// </summary>
    [HttpGet("list")]
    [ProducesResponseType(typeof(List<LocationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<LocationDto>>> GetList([FromQuery] GetLocationListQuery query)
    {
        return await Mediator.Send(query);
    }
    
    /// <summary>
    /// Retrieves the location hierarchy in a tree structure
    /// </summary>
    [HttpGet("hierarchical")]
    [ProducesResponseType(typeof(List<LocationHierarchyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<LocationHierarchyDto>>> GetHierarchy([FromQuery] GetLocationHierarchyQuery query)
    {
        return await Mediator.Send(query);
    }
} 