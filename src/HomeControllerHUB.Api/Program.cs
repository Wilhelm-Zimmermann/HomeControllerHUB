using System.Text.Json.Serialization;
using FluentValidation.AspNetCore;
using HomeControllerHUB.Api;
using HomeControllerHUB.Api.Controllers;
using HomeControllerHUB.Api.Middlewares;
using HomeControllerHUB.Application;
using HomeControllerHUB.Domain;
using HomeControllerHUB.Globalization;
using HomeControllerHUB.Infra;
using HomeControllerHUB.Infra.Settings;
using HomeControllerHUB.Infra.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Program>());
builder.Services.AddApplicationServices();
builder.Services.ConfigureDatabase(builder.Configuration);
builder.Services.AddGlobalizationServices();
builder.Services.AddInfra(builder.Configuration);
builder.Services.AddDomainServices();

builder.Services.AddSingleton<ApplicationSettings>(sp =>
{
    var settings = new ApplicationSettings();
    builder.Configuration.GetSection("ApplicationSettings").Bind(settings);
    return settings;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
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

app.UseGlobalization();
app.UseRouting();
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerAndUI(ApiVersionListConstants.ApiVersions);

}

app.UseMiddleware<ErrorHandlingMiddleware>();
app.IntializeDatabase();
app.UseCors(allowedOrigins);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();