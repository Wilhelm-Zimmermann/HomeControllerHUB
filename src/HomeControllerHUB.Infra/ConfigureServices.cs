using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Infra.DataInitializers;
using HomeControllerHUB.Infra.Interceptors;
using HomeControllerHUB.Infra.Services;
using HomeControllerHUB.Infra.Settings;
using HomeControllerHUB.Infra.Swagger;
using HomeControllerHUB.Shared.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeControllerHUB.Infra;

public static class ConfigureServices
{
    public static IServiceCollection AddInfra(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddHttpClient();
        
        // Named HttpClient for Mailgun
        services.AddHttpClient("Mailgun");
        
        services.AddScoped<BaseEntityInterceptor>();
        services.AddScoped<NormalizedInterceptor>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ApiUserManager>();
        services.AddScoped<ApplicationSettings>();
        services.AddScoped<SignInManager<ApplicationUser>>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDateTime, DateTimeService>();
        services.AddInitializers();
        
        // Register background services
        services.AddHostedService<DataRetentionService>();
        
        // Add configuration settings
        var appSettings = configuration.GetSection(nameof(ApplicationSettings)).Get<ApplicationSettings>();
        services.Configure<ApplicationSettings>(configuration.GetSection(nameof(ApplicationSettings)));
        
        // Configure Email Settings
        services.Configure<EmailSettings>(configuration.GetSection(nameof(EmailSettings)));
        
        services.AddSwagger(new List<string>() {"1"}, "OAuth2", appSettings);
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        return services;
    }
    
    private static IServiceCollection AddInitializers(this IServiceCollection services)
    {
        services.AddTransient<IDataInitializer, EstablishmentDataInitializer>(); // 1
        services.AddTransient<IDataInitializer, ProfileDataInitializer>(); // 2
        services.AddTransient<IDataInitializer, UserDataInitializer>(); // 3
        services.AddTransient<IDataInitializer, DomainDataInitializer>(); // 4
        services.AddTransient<IDataInitializer, PrivilegeDataInitializer>(); // 5
        services.AddTransient<IDataInitializer, GenericDataInitializer>(); // 6
        services.AddTransient<IDataInitializer, MenuDataInitializer>(); // 7

        return services;
    }
    
    public static IApplicationBuilder IntializeDatabase(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var settings = scope.ServiceProvider.GetService<IConfiguration>().GetSection(nameof(ApplicationSettings)).Get<ApplicationSettings>();

        if (string.IsNullOrEmpty(settings.InitializeDataBase) || settings.InitializeDataBase.ToLower() == bool.TrueString.ToLower())
        {
            var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>(); //Service locator

            if (dbContext is not null)
            {
                var dataInitializers = scope.ServiceProvider.GetServices<IDataInitializer>();
                foreach (var dataInitializer in dataInitializers.ToList().OrderBy(k => k.Order))
                {
                    dataInitializer.InitializeData();
                }
            }
        }

        return app;
    }
}