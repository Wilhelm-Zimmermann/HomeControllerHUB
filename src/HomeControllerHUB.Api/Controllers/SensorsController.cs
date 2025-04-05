using HomeControllerHUB.Application.Sensors.Commands.CreateSensor;
using HomeControllerHUB.Application.Sensors.Commands.DeleteSensor;
using HomeControllerHUB.Application.Sensors.Commands.UpdateSensor;
using HomeControllerHUB.Application.Sensors.Queries;
using HomeControllerHUB.Application.Sensors.Queries.GetSensor;
using HomeControllerHUB.Application.Sensors.Queries.GetSensorAlerts;
using HomeControllerHUB.Application.Sensors.Queries.GetSensorList;
using HomeControllerHUB.Application.Sensors.Queries.GetSensorReadings;
using HomeControllerHUB.Application.Sensors.Queries.GetSensors;
using HomeControllerHUB.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Asp.Versioning;

namespace HomeControllerHUB.Api.Controllers;

[ApiVersion(ApiConstants.ApiVersion1)]
public class SensorsController : ApiControllerBase
{
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(BaseEntityResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Guid>> Create(CreateSensorCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(Get), new { id = result }, new BaseEntityResponse { Id = result });
    }
    
    [HttpPut]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(UpdateSensorCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }
    
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete([FromBody] DeleteSensorCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SensorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SensorDto>> Get(Guid id)
    {
        return await Mediator.Send(new GetSensorQuery { Id = id });
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<SensorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedList<SensorDto>>> Get([FromQuery] GetSensorsQuery query)
    {
        return await Mediator.Send(query);
    }
    
    [HttpGet("list")]
    [ProducesResponseType(typeof(List<SensorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<SensorDto>>> GetList([FromQuery] GetSensorListQuery query)
    {
        return await Mediator.Send(query);
    }
    
    [HttpGet("{id}/readings")]
    [ProducesResponseType(typeof(PaginatedList<SensorReadingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedList<SensorReadingDto>>> GetReadings(Guid id, [FromQuery] GetSensorReadingsQuery query)
    {
        query.SensorId = id;
        return await Mediator.Send(query);
    }
    
    [HttpGet("{id}/alerts")]
    [ProducesResponseType(typeof(PaginatedList<SensorAlertDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedList<SensorAlertDto>>> GetAlerts(Guid id, [FromQuery] GetSensorAlertsQuery query)
    {
        query.SensorId = id;
        return await Mediator.Send(query);
    }
} 