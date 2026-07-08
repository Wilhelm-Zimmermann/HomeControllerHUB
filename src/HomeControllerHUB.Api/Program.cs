using System.Text.Json.Serialization;
using FluentValidation.AspNetCore;
using HomeControllerHUB.Api;
using HomeControllerHUB.Api.Controllers;
using HomeControllerHUB.Api.Extensions;
using HomeControllerHUB.Api.HealthChecks;
using HomeControllerHUB.Api.HostedServices;
using HomeControllerHUB.Api.Middlewares;
using HomeControllerHUB.Application;
using HomeControllerHUB.Application.Sensors.Commands.MonitorSensorHealth;
using HomeControllerHUB.Domain;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra;
using HomeControllerHUB.Infra.Constants;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Infra.Settings;
using HomeControllerHUB.Infra.Swagger;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

Console.WriteLine("Home Controller HUB - ENV: " + builder.Environment.EnvironmentName);

builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Program>());
builder.Services.AddApplicationServices();
builder.Services.ConfigureDatabase(builder.Configuration, builder.Environment);
builder.Services.AddGlobalizationServices();
builder.Services.AddInfra(builder.Configuration);
builder.Services.AddDomainServices();
builder.Services.AddHomeControllerHubMessageBus(builder.Configuration, builder.Environment);
builder.Services.Configure<SensorHealthMonitoringOptions>(
    builder.Configuration.GetSection(SensorHealthMonitoringOptions.SectionName));
builder.Services.AddHostedService<SensorHealthMonitoringHostedService>();
builder.Services.AddHealthChecks()
    .AddCheck("application", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Application is running"), tags: new[] { "live", "ready" })
    .AddCheck<ApplicationDbContextHealthCheck>("database", tags: new[] { "ready" })
    .AddCheck<RabbitMqHealthCheck>("rabbitmq", tags: new[] { "ready" });
builder.Services.AddHomeControllerHubRateLimiting();

builder.Services.AddSingleton<ApplicationSettings>(sp =>
{
    var settings = new ApplicationSettings();
    builder.Configuration.GetSection("ApplicationSettings").Bind(settings);
    return settings;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCustomOpenApi();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddPolicy(GeneralConfigs.CORS, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();
using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
if (dbContext.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory" && dbContext.Database.IsRelational())
{
    dbContext.Database.Migrate();
}

app.UseGlobalization();
app.UseRouting();
app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.UseSwaggerAndUI(ApiVersionListConstants.ApiVersions);

}

app.UseMiddleware<ErrorHandlingMiddleware>();
app.IntializeDatabase();
app.UseCors(GeneralConfigs.CORS);
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.MapHealthChecks("/health", HealthCheckResponseWriter.ForTags());
app.MapHealthChecks("/health/live", HealthCheckResponseWriter.ForTags("live"));
app.MapHealthChecks("/health/ready", HealthCheckResponseWriter.ForTags("ready"));
app.MapControllers();

app.Run();

public partial class Program;
