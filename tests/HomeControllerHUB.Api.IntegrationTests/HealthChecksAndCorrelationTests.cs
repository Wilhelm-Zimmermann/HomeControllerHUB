using HomeControllerHUB.Api.Middlewares;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Infra.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HomeControllerHUB.Api.IntegrationTests;

public class HealthChecksAndCorrelationTests : IClassFixture<HealthChecksWebApplicationFactory>
{
    private readonly HealthChecksWebApplicationFactory _factory;

    public HealthChecksAndCorrelationTests(HealthChecksWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Request_without_correlation_id_returns_generated_header()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        Assert.True(Guid.TryParse(response.Headers.GetValues("X-Correlation-ID").Single(), out _));
    }

    [Fact]
    public async Task Request_with_correlation_id_preserves_header_value()
    {
        const string correlationId = "test-correlation-123";
        var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.Add("X-Correlation-ID", correlationId);

        var response = await client.SendAsync(request);

        Assert.Equal(correlationId, response.Headers.GetValues("X-Correlation-ID").Single());
    }

    [Fact]
    public async Task Health_live_returns_healthy_without_database_check()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/live");
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("\"status\":\"Healthy\"", body);
        Assert.Contains("\"name\":\"application\"", body);
        Assert.DoesNotContain("\"name\":\"database\"", body);
    }

    [Fact]
    public async Task Health_ready_returns_application_and_database_checks()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/ready");
        var body = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();
        Assert.Contains("\"status\":\"Healthy\"", body);
        Assert.Contains("\"name\":\"application\"", body);
        Assert.Contains("\"name\":\"database\"", body);
        Assert.Contains("\"name\":\"rabbitmq\"", body);
    }

    [Fact]
    public async Task Error_response_returns_correlation_id_header()
    {
        const string correlationId = "error-correlation-123";
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = correlationId;

        RequestDelegate throwingEndpoint = _ => throw new InvalidOperationException("failed");
        var errorHandlingMiddleware = new ErrorHandlingMiddleware(
            throwingEndpoint,
            NullLogger<ErrorHandlingMiddleware>.Instance);
        var correlationIdMiddleware = new CorrelationIdMiddleware(
            errorHandlingMiddleware.Invoke,
            NullLogger<CorrelationIdMiddleware>.Instance);

        await correlationIdMiddleware.Invoke(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal(correlationId, context.Response.Headers[CorrelationIdMiddleware.HeaderName].Single());
    }
}

public class HealthChecksWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly InMemoryDatabaseRoot DatabaseRoot = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ApplicationSettings:InitializeDataBase", "false");
        builder.ConfigureLogging(logging => logging.ClearProviders());

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            var hostedServicesToRemove = services
                .Where(descriptor => descriptor.ServiceType == typeof(IHostedService)
                                     && descriptor.ImplementationType == typeof(DataRetentionService))
                .ToList();

            foreach (var descriptor in hostedServicesToRemove)
            {
                services.Remove(descriptor);
            }

            var inMemoryDatabaseServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options
                    .UseInMemoryDatabase("HomeControllerHUB-IntegrationTests", DatabaseRoot)
                    .UseInternalServiceProvider(inMemoryDatabaseServiceProvider);
            });
        });
    }
}
