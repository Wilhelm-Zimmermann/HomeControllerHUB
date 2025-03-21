using HomeControllerHUB.Infra.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace HomeControllerHUB.Api;

public static class ConfigureServices
{
    public static IServiceCollection ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("Npgsql"), x => x.MigrationsAssembly("HomeControllerHUB.Api"));
            options.EnableSensitiveDataLogging();
        });
        
        return services;
    }
}