using System.Text.Json.Serialization;
using HomeControllerHUB.Api;
using HomeControllerHUB.Application;
using HomeControllerHUB.Infra;
using HomeControllerHUB.Infra.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddOpenApi();
builder.Services.AddInfra(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.ConfigureDatabase(builder.Configuration);
builder.Services.AddSingleton<ApplicationSettings>(sp =>
{
    var settings = new ApplicationSettings();
    builder.Configuration.GetSection("ApplicationSettings").Bind(settings);
    return settings;
});
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

app.IntializeDatabase();
app.UseRouting();
app.UseCors(allowedOrigins);
app.UseAuthentication();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();