using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Interfaces;
using HomeControllerHUB.Infra.DatabaseContext;
using HomeControllerHUB.Infra.DataInitializers;
using HomeControllerHUB.Infra.Interceptors;
using HomeControllerHUB.Infra.Services;
using HomeControllerHUB.Infra.Settings;
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
        services.AddScoped<BaseEntityInterceptor>();
        services.AddScoped<NormalizedInterceptor>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ApiUserManager>();
        services.AddScoped<ApplicationSettings>();
        services.AddScoped<SignInManager<ApplicationUser>>();
        services.AddTransient<ICurrentUserService, CurrentUserService>();
        services.AddJwtAuthentication();
        services.AddInitializers();
        return services;
    }
    
    private static IServiceCollection AddInitializers(this IServiceCollection services)
    {
        services.AddTransient<IDataInitializer, EstablishmentDataInitializer>();
        services.AddTransient<IDataInitializer, ProfileDataInitializer>();
        services.AddTransient<IDataInitializer, UserDataInitializer>();
        // services.AddTransient<IDataInitializer, DomainDataInitializer>();
        // services.AddTransient<IDataInitializer, PrivilegeDataInitializer>();
        // services.AddTransient<IDataInitializer, MenuDataInitializer>();
        // services.AddTransient<IDataInitializer, ProfilePrivilegeDataInitializer>();
        // services.AddTransient<IDataInitializer, GenericDataInitializer>();

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