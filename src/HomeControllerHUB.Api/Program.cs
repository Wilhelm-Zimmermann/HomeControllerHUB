using System.Text.Json.Serialization;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using HomeControllerHUB.Api;
using HomeControllerHUB.Application;
using HomeControllerHUB.Infra;
using HomeControllerHUB.Infra.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddInfra(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.ConfigureDatabase(builder.Configuration);

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

app.UseRouting();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "v1");
    });
}

app.IntializeDatabase();
app.UseCors(allowedOrigins);
app.UseAuthentication();
app.MapControllers();

app.Run();