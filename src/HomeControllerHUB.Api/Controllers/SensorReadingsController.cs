using System.Net.Mime;
using System.Text.Json;
using Asp.Versioning;
using HomeControllerHUB.Api.Extensions;
using HomeControllerHUB.Api.Middlewares;
using HomeControllerHUB.Application.Sensors.Commands.IngestSensorReading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HomeControllerHUB.Api.Controllers;

[ApiVersion(ApiConstants.ApiVersion1)]
public class SensorReadingsController : ApiControllerBase
{
    [HttpPost("ingest")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.SensorIngestionPolicy)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IngestSensorReadingResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IngestSensorReadingResponse>> Ingest(
        [FromHeader(Name = "X-Api-Key")] string? apiKey,
        [FromBody] IngestSensorReadingRequest request)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Authentication failed",
                Detail = "Missing X-Api-Key header",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var command = new IngestSensorReadingCommand
        {
            ApiKey = apiKey,
            MessageId = request.MessageId,
            DeviceId = request.DeviceId,
            Timestamp = request.Timestamp,
            Value = request.Value,
            Unit = request.Unit,
            BatteryLevel = request.BatteryLevel,
            RawData = SerializeRawData(request.RawData),
            CorrelationId = GetCorrelationId()
        };

        return Accepted(await Mediator.Send(command));
    }

    private static string? SerializeRawData(JsonElement? rawData)
    {
        if (rawData is null || rawData.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        return rawData.Value.ValueKind == JsonValueKind.String
            ? rawData.Value.GetString()
            : rawData.Value.GetRawText();
    }

    private string? GetCorrelationId()
    {
        return HttpContext.Items.TryGetValue(CorrelationIdMiddleware.ItemKey, out var correlationId)
               && correlationId is not null
            ? correlationId.ToString()
            : HttpContext.Response.Headers[CorrelationIdMiddleware.HeaderName].FirstOrDefault();
    }
}

public class IngestSensorReadingRequest
{
    public string MessageId { get; set; } = null!;
    public string DeviceId { get; set; } = null!;
    public DateTime? Timestamp { get; set; }
    public double? Value { get; set; }
    public string? Unit { get; set; }
    public double? BatteryLevel { get; set; }
    public JsonElement? RawData { get; set; }
}
