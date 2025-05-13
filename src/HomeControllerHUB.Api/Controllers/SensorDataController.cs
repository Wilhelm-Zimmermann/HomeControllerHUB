using HomeControllerHUB.Application.Sensors.Commands.SubmitSensorReading;
using HomeControllerHUB.Application.Sensors.Commands.SubmitSensorReadingBatch;
using HomeControllerHUB.Application.Sensors.Commands.UpdateSensorStatus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Asp.Versioning;

namespace HomeControllerHUB.Api.Controllers;

[ApiVersion(ApiConstants.ApiVersion1)]
public class SensorDataController : ApiControllerBase
{
    [HttpPost("readings")]
    [AllowAnonymous] // This endpoint needs to be accessible by IoT devices
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> SubmitReading(SubmitSensorReadingCommand command)
    {
        // This will validate the API key in the handler
        await Mediator.Send(command);
        return Ok();
    }
    
    [HttpPost("readings/batch")]
    [AllowAnonymous] // This endpoint needs to be accessible by IoT devices
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> SubmitReadingBatch(SubmitSensorReadingBatchCommand command)
    {
        // This will validate the API key in the handler
        await Mediator.Send(command);
        return Ok();
    }
    
    [HttpPost("status")]
    [AllowAnonymous] // This endpoint needs to be accessible by IoT devices
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateStatus(UpdateSensorStatusCommand command)
    {
        // This will validate the API key in the handler
        await Mediator.Send(command);
        return Ok();
    }
} 