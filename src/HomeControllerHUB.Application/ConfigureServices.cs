using System.Reflection;
using FluentValidation.AspNetCore;
using HomeControllerHUB.Application.Profiles.Queries;
using HomeControllerHUB.Domain.Entities;
using HomeControllerHUB.Domain.Mappings;
using HomeControllerHUB.Domain.Models;
using HomeControllerHUB.Infra.Interceptors;
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
        
        services.AddAutoMapper(config => config.AddProfile(new MappingProfile(typeof(BaseEntityResponse).Assembly)));
        services.AddAutoMapper(config => config.AddProfile(new MappingProfile(typeof(GetProfilePaginatedDto).Assembly)));
        services.AddAutoMapper(typeof(Domain.ConfigureServices).Assembly);
        
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));
        services.AddAutoMapper(typeof(Domain.ConfigureServices).Assembly);
        services.AddFluentValidationAutoValidation();
        return services;
    }
}