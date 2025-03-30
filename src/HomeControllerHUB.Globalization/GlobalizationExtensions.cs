using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;

namespace HomeControllerHUB.Globalization;

public static class GlobalizationExtensions
{

    public static void AddGlobalizationServices(this IServiceCollection services)
    {
        services.AddLocalization(o =>
            // We will put our translations in a folder called Resources
            o.ResourcesPath = "Resources");

        services.TryAddSingleton<IStringLocalizerFactory, ResourceManagerStringLocalizerFactory>();
        services.TryAddTransient<IStringLocalizer<SharedResource>, StringLocalizer<SharedResource>>();
        services.TryAddTransient<ISharedResource, SharedResource>();

    }

    public static void UseGlobalization(this IApplicationBuilder app)
    {
        var supportedCultures = new[]
        {
            CultureNames.PortugueseCulture,
            CultureNames.EnglishCulture,
            CultureNames.SpanishCulture,
        };

        var localizationOptions = new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(CultureNames.Portuguese),
            // Formatting numbers, dates, etc.
            SupportedCultures = supportedCultures,
            // UI strings that we have localized.
            SupportedUICultures = supportedCultures
        };

        var cookieProvider = localizationOptions.RequestCultureProviders
            .OfType<CookieRequestCultureProvider>()
            .First();

        // Set the new cookie name
        cookieProvider.CookieName = "UserCulture";

        localizationOptions.RequestCultureProviders.Add(new AcceptLanguageHeaderRequestCultureProvider());

        app.UseRequestLocalization(localizationOptions);
    }
}
