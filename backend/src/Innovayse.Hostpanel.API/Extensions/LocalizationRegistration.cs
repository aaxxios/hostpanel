using Innovayse.Hostpanel.Application.Common.Settings;
using Innovayse.Hostpanel.Application.Resources;
using Innovayse.Hostpanel.Shared.Contracts.Interfaces;
using Innovayse.Hostpanel.Shared.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.API.Extensions;

/// <summary>
/// Registers the pieces every request's localization depends on: the default and supported
/// locales, the localization middleware's own services, and the resolver
/// <c>BaseApiController.MapFailure</c> uses to turn an error code into a sentence.
/// </summary>
public static class LocalizationRegistration
{
    /// <summary>Adds hostpanel's localization services.</summary>
    /// <param name="services">The container.</param>
    /// <param name="configuration">Host configuration, read here for the default locale only.</param>
    /// <returns>The container, for chaining.</returns>
    public static IServiceCollection AddHostpanelLocalization(this IServiceCollection services, IConfiguration configuration)
    {
        var defaultLocale = configuration[LocaleOptions.ConfigurationKey];
        services.AddOptions<LocaleOptions>()
            .Configure(o =>
            {
                if (!string.IsNullOrWhiteSpace(defaultLocale))
                {
                    o.DefaultLocale = defaultLocale;
                }
            })
            .Validate(
                o => o.DefaultLocale.Length is 2 or 5,
                $"{LocaleOptions.ConfigurationKey} must be a locale code such as en or en-US.")
            .ValidateOnStart();

        // The refusal sentences in Innovayse.Hostpanel.Application/Resources/ApplicationValidationMessages*.resx. No
        // ResourcesPath: the resx files sit in a Resources/ folder whose name is already part of the
        // ApplicationValidationMessages marker type's namespace, and setting it would make the factory look for
        // Resources.Resources.ApplicationValidationMessages instead.
        services.AddLocalization();

        // What XLanguageRequestCultureProvider (Shared/Localization/) asks whether a given
        // X-Language value is one this deployment answers in. SupportedLanguages (en/ru/hy) is the
        // same set LocaleOptions.SupportedLocales names for the culture list below, by construction.
        services.AddSingleton<ISupportedLanguagesProvider, PlatformSupportedLanguagesProvider>();

        // BaseApiController.MapFailure resolves a refusal's sentence through this, by error code, from
        // Application/Resources/ErrorMessages*.resx in the request's language.
        services.AddScoped<IErrorMessageResolver, ErrorMessageResolver>();

        return services;
    }
}
