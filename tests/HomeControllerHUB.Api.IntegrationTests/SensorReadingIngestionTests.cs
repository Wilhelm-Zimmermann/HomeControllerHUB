using System.Net;
using System.Text;
using System.Text.Json;
using HomeControllerHUB.Api.Middlewares;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Infra.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HomeControllerHUB.Api.IntegrationTests;

[Collection(ApiIntegrationCollection.Name)]
public class SensorReadingIngestionTests
{
    private const string ApiKey = "integration-sensor-key";
    private readonly HealthChecksWebApplicationFactory _factory;

    public SensorReadingIngestionTests(HealthChecksWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Ingest_without_api_key_returns_unauthorized()
    {
        var client = _factory.CreateClient();
        using var request = CreateIngestRequest("missing-header-device", "missing-header-message", includeApiKey: false);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.True(response.Headers.Contains(CorrelationIdMiddleware.HeaderName));
    }

    [Fact]
    public async Task Ingest_with_valid_api_key_returns_accepted_and_queues_message()
    {
        var deviceId = $"device-{Guid.NewGuid():N}";
        var client = _factory.CreateClient();
        await SeedSensorAsync(deviceId);

        using var request = CreateIngestRequest(deviceId, "message-1");
        var response = await client.SendAsync(request);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.True(response.StatusCode == HttpStatusCode.Accepted, body);
        Assert.Equal("Queued", json.RootElement.GetProperty("status").GetString());
        Assert.True(response.Headers.Contains(CorrelationIdMiddleware.HeaderName));

        var processed = await WaitForReadingAsync(deviceId, "message-1");
        Assert.True(processed);
    }

    [Fact]
    public async Task Ingest_with_duplicate_message_returns_duplicate_status()
    {
        var deviceId = $"device-{Guid.NewGuid():N}";
        var client = _factory.CreateClient();
        await SeedSensorAsync(deviceId);

        using var firstRequest = CreateIngestRequest(deviceId, "duplicate-message");
        var firstResponse = await client.SendAsync(firstRequest);
        firstResponse.EnsureSuccessStatusCode();
        Assert.True(await WaitForReadingAsync(deviceId, "duplicate-message"));

        using var duplicateRequest = CreateIngestRequest(deviceId, "duplicate-message");
        var duplicateResponse = await client.SendAsync(duplicateRequest);
        var body = await duplicateResponse.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal(HttpStatusCode.Accepted, duplicateResponse.StatusCode);
        Assert.Equal("Duplicate", json.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Ingest_response_contains_correlation_id()
    {
        const string correlationId = "sensor-ingest-correlation-123";
        var deviceId = $"device-{Guid.NewGuid():N}";
        var client = _factory.CreateClient();
        await SeedSensorAsync(deviceId);

        using var request = CreateIngestRequest(deviceId, "correlation-message");
        request.Headers.Add(CorrelationIdMiddleware.HeaderName, correlationId);
        var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        Assert.Equal(correlationId, response.Headers.GetValues(CorrelationIdMiddleware.HeaderName).Single());
    }

    private static HttpRequestMessage CreateIngestRequest(
        string deviceId,
        string messageId,
        bool includeApiKey = true)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/SensorReadings/ingest")
        {
            Content = new StringContent($$"""
            {
              "messageId": "{{messageId}}",
              "deviceId": "{{deviceId}}",
              "timestamp": "2026-07-02T12:00:00Z",
              "value": 29.5,
              "unit": "C",
              "batteryLevel": 87,
              "rawData": {
                "firmware": "1.0.3"
              }
            }
            """, Encoding.UTF8, "application/json")
        };

        if (includeApiKey)
        {
            request.Headers.Add("X-Api-Key", ApiKey);
        }

        return request;
    }

    private async Task SeedSensorAsync(string deviceId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var establishment = new Establishment
        {
            Code = $"EST-{Guid.NewGuid():N}"[..20],
            Name = "Integration establishment",
            SiteName = "Integration site",
            Document = "12345678901234",
            Enable = true,
            IsMaster = true
        };

        var location = new Location
        {
            Establishment = establishment,
            EstablishmentId = establishment.Id,
            Name = "Integration room"
        };

        var sensor = new Sensor
        {
            Establishment = establishment,
            EstablishmentId = establishment.Id,
            Location = location,
            Name = "Integration sensor",
            DeviceId = deviceId,
            Type = SensorType.Temperature,
            Model = "ESP32",
            ApiKey = ApiKey,
            IsActive = true,
            LastCommunication = DateTime.UtcNow.AddMinutes(-10)
        };

        context.Sensors.Add(sensor);
        await context.SaveChangesAsync();
    }

    private async Task<bool> WaitForReadingAsync(string deviceId, string messageId)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var exists = await context.SensorReadings.AnyAsync(
                reading => reading.Sensor.DeviceId == deviceId && reading.MessageId == messageId);

            if (exists)
            {
                return true;
            }

            await Task.Delay(100);
        }

        return false;
    }
}
