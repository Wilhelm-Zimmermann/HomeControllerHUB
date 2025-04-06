using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using Asp.Versioning;
using HomeControllerHUB.Infra.Settings;
using HomeControllerHUB.Shared.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace HomeControllerHUB.Infra.Swagger;

public static class SwaggerConfigurationExtensions
{
    public static void AddSwagger(this IServiceCollection services)
    {
        services.AddSwagger();
    }

    public static void AddSwagger(this IServiceCollection services, List<string> versions, string authSchema = "OAuth2",
        ApplicationSettings? appSettings = null, Dictionary<string, string>? scopes = null)
    {
        Assert.NotNull(services, nameof(services));
        //More info : https://github.com/mattfrear/Swashbuckle.AspNetCore.Filters

        #region AddSwaggerExamples

        //Add services to use Example Filters in swagger
        //If you want to use the Request and Response example filters (and have called options.ExampleFilters() above), then you MUST also call
        //This method to register all ExamplesProvider classes form the assembly
        //services.AddSwaggerExamplesFromAssemblyOf<PersonRequestExample>();

        //We call this method for by reflection with the Startup type of entry assmebly (MyApi assembly)
        var mainAssembly = Assembly.GetEntryAssembly(); // => MyApi project assembly
        if (mainAssembly is not null)
        {
            var mainType = mainAssembly.GetExportedTypes()[0];

            const string methodName = nameof(Swashbuckle.AspNetCore.Filters.ServiceCollectionExtensions
                .AddSwaggerExamplesFromAssemblyOf);
            MethodInfo? method = typeof(Swashbuckle.AspNetCore.Filters.ServiceCollectionExtensions).GetRuntimeMethods()
                .FirstOrDefault(x => string.Equals(x.Name, methodName, StringComparison.Ordinal) && x.IsGenericMethod);
            if (method is not null)
            {
                MethodInfo generic = method.MakeGenericMethod(mainType);
                generic.Invoke(null, new[] { services });
            }
        }

        #endregion

        //Add services and configuration to use swagger
        _ = services.AddSwaggerGen(options =>
        {
            var title = "Zanini API Documentation";
            if (appSettings is not null && appSettings.SwaggerSettings is not null &&
                !string.IsNullOrWhiteSpace(appSettings.SwaggerSettings.Title))
                title = appSettings.SwaggerSettings.Title.Trim();

            foreach (var version in versions)
            {
                options.SwaggerDoc($"v{version}",
                    new OpenApiInfo { Version = $"v{version}", Title = $"{title} - V{version}" });
            }

            #region Filters

            //Enable to use [SwaggerRequestExample] & [SwaggerResponseExample]
            options.ExampleFilters();

            //It doesn't work anymore in recent versions because of replacing Swashbuckle.AspNetCore.Examples with Swashbuckle.AspNetCore.Filters
            //Adds an Upload button to endpoints which have [AddSwaggerFileUploadButton]
            //options.OperationFilter<AddFileParamTypesOperationFilter>();

            //Set summary of action if not already set
            options.OperationFilter<ApplySummariesOperationFilter>();

            #region Add UnAuthorized to Response

            //Add 401 response and security requirements (Lock icon) to actions that need authorization
            if (authSchema is not null && authSchema == "OAuth2")
                options.OperationFilter<UnauthorizedResponsesOperationFilter>(true, "OAuth2");
            else if (authSchema is not null && authSchema == "Bearer")
                options.OperationFilter<UnauthorizedResponsesOperationFilter>(true, "Bearer");

            #endregion

            #region Add Jwt Authentication

            //Add Lockout icon on top of swagger ui page to authenticate
            // normalize url
            if (authSchema is not null && authSchema == "OAuth2")
            {
                var identity = appSettings.HostSettings.IdentityUrl.EndsWith('/')
                    ? appSettings.HostSettings.IdentityUrl.Remove(appSettings.HostSettings.IdentityUrl.Length - 1)
                    : appSettings.HostSettings.IdentityUrl;

                //OAuth2Scheme
                options.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Password = new OpenApiOAuthFlow
                        {
                            TokenUrl = new Uri($"{identity}/api/v1/Users/Token", UriKind.Absolute),
                            Scopes = scopes
                        }
                    }
                });
            }
            else if (authSchema is not null && authSchema == "Bearer")
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter token",
                    Name = "Authorization",
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
            }

            #endregion

            #region Versioning

            // Remove version parameter from all Operations
            options.OperationFilter<RemoveVersionParameters>();

            //set version "api/v{version}/[controller]" from current swagger doc verion
            options.DocumentFilter<SetVersionInPaths>();

            //Seperate and categorize end-points by doc version
            options.DocInclusionPredicate((docName, apiDesc) =>
            {
                if (!apiDesc.TryGetMethodInfo(out MethodInfo? methodInfo))
                {
                    return false;
                }

                if (methodInfo is not null)
                {
                    var versions = methodInfo?.DeclaringType?
                        .GetCustomAttributes<ApiVersionAttribute>(true)
                        .SelectMany(attr => attr.Versions);

                    return versions.Any(v => $"v{v}" == docName);
                }

                return false;
            });

            #endregion

            //If use FluentValidation then must be use this package to show validation in swagger (MicroElements.Swashbuckle.FluentValidation)
            //options.AddFluentValidationRules();

            #endregion

            // Configure a custom schema ID selector to include namespace for disambiguating types with the same name
            options.CustomSchemaIds(type =>
            {
                // Get full type name including namespace
                var fullName = type.FullName;
                if (fullName == null) return type.Name;

                // For nested generic types, extract the base name and parameters
                if (type.IsGenericType)
                {
                    var prefix = fullName.Substring(0, fullName.IndexOf('`'));
                    var genericArgs = string.Join("And", type.GetGenericArguments().Select(t => t.Name));
                    return $"{prefix.Replace("+", ".").Replace(".", "_")}_Of_{genericArgs}";
                }
                
                // Replace problematic characters and namespace separators with underscore
                return fullName.Replace("+", ".").Replace(".", "_");
            });
        });
    }

    public static IServiceCollection AddCustomOpenApi(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            // Include XML Comments
            var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
            foreach (var xmlFile in xmlFiles)
            {
                options.IncludeXmlComments(xmlFile);
            }
            
            // Configure a custom schema ID selector to include namespace for disambiguating types with the same name
            options.CustomSchemaIds(type =>
            {
                // Get full type name including namespace
                var fullName = type.FullName;
                if (fullName == null) return type.Name;

                // For nested generic types, extract the base name and parameters
                if (type.IsGenericType)
                {
                    var prefix = fullName.Substring(0, fullName.IndexOf('`'));
                    var genericArgs = string.Join("And", type.GetGenericArguments().Select(t => t.Name));
                    return $"{prefix.Replace("+", ".").Replace(".", "_")}_Of_{genericArgs}";
                }
                
                // Replace problematic characters and namespace separators with underscore
                return fullName.Replace("+", ".").Replace(".", "_");
            });
        });
        
        return services;
    }

    public static IApplicationBuilder UseSwaggerAndUI(this IApplicationBuilder app, List<string> versions)
    {
        Assert.NotNull(app, nameof(app));

        //More info : https://github.com/domaindrivendev/Swashbuckle.AspNetCore

        var conf = app.ApplicationServices.CreateScope().ServiceProvider.GetService<IConfiguration>();
        var appSettings = conf.GetSection(nameof(ApplicationSettings)).Get<ApplicationSettings>();

        var title = "Zanini API Documentation";
        if (appSettings is not null && appSettings.SwaggerSettings is not null &&
            !string.IsNullOrWhiteSpace(appSettings.SwaggerSettings.Title))
            title = appSettings.SwaggerSettings.Title.Trim();

        //Swagger middleware for generate "Open API Documentation" in swagger.json
        app.UseSwagger( /*options =>
            {
                options.RouteTemplate = "api-docs/{documentName}/swagger.json";
            }*/);

        //Swagger middleware for generate UI from swagger.json
        app.UseSwaggerUI(options =>
        {
            foreach (var version in versions)
            {
                options.SwaggerEndpoint($"/swagger/v{version}/swagger.json", $"V{version}");
            }

            #region Customizing

            //// Display
            //options.DefaultModelExpandDepth(2);
            //options.DefaultModelRendering(ModelRendering.Model);
            //options.DefaultModelsExpandDepth(-1);
            //options.DisplayOperationId();
            //options.DisplayRequestDuration();
            options.DocExpansion(DocExpansion.None);
            //options.EnableDeepLinking();
            //options.EnableFilter();
            //options.MaxDisplayedTags(5);
            //options.ShowExtensions();

            //// Network
            //options.EnableValidator();
            //options.SupportedSubmitMethods(SubmitMethod.Get);

            //// Other
            //options.DocumentTitle = "CustomUIConfig";
            // options.InjectStylesheet("/ext/custom-stylesheet.css");
            //options.InjectJavascript("/ext/custom-javascript.js");
            //options.RoutePrefix = "api-docs";

            #endregion
        });

        return app;
    }
}