using System.Text.Json.Serialization;
using FluentValidation.AspNetCore;
using HomeControllerHUB.Api;
using HomeControllerHUB.Api.Controllers;
using HomeControllerHUB.Api.Extensions;
using HomeControllerHUB.Api.HealthChecks;
using HomeControllerHUB.Api.Middlewares;
using HomeControllerHUB.Application;
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
builder.Services.ConfigureDatabase(builder.Configuration);
builder.Services.AddGlobalizationServices();
builder.Services.AddInfra(builder.Configuration);
builder.Services.AddDomainServices();
builder.Services.AddHealthChecks()
    .AddCheck("application", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Application is running"), tags: new[] { "live", "ready" })
    .AddCheck<ApplicationDbContextHealthCheck>("database", tags: new[] { "ready" });
builder.Services.AddHomeControllerHubRateLimiting();

builder.Services.AddSingleton<ApplicationSettings>(sp =>
{
    var settings = new ApplicationSettings();
    builder.Configuration.GetSection("ApplicationSettings").Bind(settings);
    return settings;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCustomOpenApi();
const string allowedOrigins = "AllowAllOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(allowedOrigins, policy =>
    {
        policy.AllowAnyOrigin()
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

app.UseCors(GeneralConfigs.CORS);
app.UseMiddleware<ErrorHandlingMiddleware>();
app.IntializeDatabase();
app.UseCors(allowedOrigins);
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.MapHealthChecks("/health", HealthCheckResponseWriter.ForTags());
app.MapHealthChecks("/health/live", HealthCheckResponseWriter.ForTags("live"));
app.MapHealthChecks("/health/ready", HealthCheckResponseWriter.ForTags("ready"));
app.MapControllers();

app.Run();

public partial class Program;
