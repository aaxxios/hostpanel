using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Contracts.Interfaces;
using Innovayse.Hostpanel.Shared.Localization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Shared.Extensions;

/// <summary>
/// Request localization the platform way: the supported languages from <see cref="SupportedLanguages"/>
/// and <c>X-Language</c> as the only culture source.
/// </summary>
public static class PlatformLocalizationExtensions
{
    /// <summary>
    /// Registers the localizer (no resources path: a marker type's full name is its resource name),
    /// and the supported-languages provider.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>Service collection for chaining.</returns>
    public static IServiceCollection AddPlatformLocalization(this IServiceCollection services)
    {
        services.AddLocalization();
        services.AddHttpContextAccessor();
        services.AddSingleton<ISupportedLanguagesProvider, PlatformSupportedLanguagesProvider>();

        return services;
    }

    /// <summary>
    /// Sets the request culture from <c>X-Language</c> alone. The default provider chain (query
    /// string, cookie, <c>Accept-Language</c>) is removed so message language cannot diverge from
    /// content language.
    /// </summary>
    /// <param name="app">Application builder.</param>
    /// <returns>Application builder for chaining.</returns>
    public static IApplicationBuilder UsePlatformLocalization(this IApplicationBuilder app)
    {
        string[] cultures = [.. SupportedLanguages.All];

        var options = new RequestLocalizationOptions()
            .SetDefaultCulture(SupportedLanguages.Default)
            .AddSupportedCultures(cultures)
            .AddSupportedUICultures(cultures);

        options.RequestCultureProviders.Clear();
        options.RequestCultureProviders.Add(new XLanguageRequestCultureProvider());

        return app.UseRequestLocalization(options);
    }
}
