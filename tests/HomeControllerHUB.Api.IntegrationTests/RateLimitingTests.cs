using System.Net;
using System.Text.Json;
using HomeControllerHUB.Api.Middlewares;
using Microsoft.AspNetCore.Http;

namespace HomeControllerHUB.Api.IntegrationTests;

[Collection(ApiIntegrationCollection.Name)]
public class RateLimitingTests
{
    private readonly HealthChecksWebApplicationFactory _factory;

    public RateLimitingTests(HealthChecksWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Auth_endpoint_returns_too_many_requests_after_policy_limit()
    {
        const string correlationId = "rate-limit-correlation-123";
        var client = _factory.CreateClient();
        HttpResponseMessage? response = null;

        for (var i = 0; i < 10; i++)
        {
            response = await PostTokenAsync(client, correlationId);

            Assert.NotEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
        }

        response = await PostTokenAsync(client, correlationId);
        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        Assert.True(response.Headers.Contains(CorrelationIdMiddleware.HeaderName));
        Assert.Equal(correlationId, response.Headers.GetValues(CorrelationIdMiddleware.HeaderName).Single());
        Assert.Equal("Too many requests", json.RootElement.GetProperty("title").GetString());
        Assert.Equal(StatusCodes.Status429TooManyRequests, json.RootElement.GetProperty("statusCode").GetInt32());
        Assert.Equal(correlationId, json.RootElement.GetProperty("correlationId").GetString());
        Assert.False(string.IsNullOrWhiteSpace(json.RootElement.GetProperty("message").GetString()));
    }

    [Fact]
    public async Task Health_live_is_not_rate_limited()
    {
        var client = _factory.CreateClient();

        for (var i = 0; i < 15; i++)
        {
            var response = await client.GetAsync("/health/live");

            Assert.NotEqual(HttpStatusCode.TooManyRequests, response.StatusCode);
            response.EnsureSuccessStatusCode();
        }
    }

    private static async Task<HttpResponseMessage> PostTokenAsync(HttpClient client, string correlationId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Users/Token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["UserName"] = "rate-limit@example.com",
                ["Password"] = "invalid-password"
            })
        };
        request.Headers.Add(CorrelationIdMiddleware.HeaderName, correlationId);

        return await client.SendAsync(request);
    }
}
