using System.Reflection;
using HomeControllerHUB.Domain.Entities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace HomeControllerHUB.Application;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {

        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
        // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));
        return services;
    }
}