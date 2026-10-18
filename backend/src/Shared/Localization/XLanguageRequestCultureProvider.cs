using Innovayse.Hostpanel.Shared.Contracts.Interfaces;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Shared.Localization;

/// <summary>
///     Resolves the request culture (validation messages, IStringLocalizer output) from the
///     platform's <c>X-Language</c> header — the same single language source used for content.
///     Validation goes through the configuration-backed language policy resolved
///     from the request's service scope. Replaces the default ASP.NET provider chain, whose
///     Accept-Language step made message language diverge from content language.
/// </summary>
public sealed class XLanguageRequestCultureProvider : RequestCultureProvider
{
    /// <summary>
    ///     Reads a valid <c>X-Language</c> header into a culture result.
    /// </summary>
    /// <param name="httpContext">Current HTTP context.</param>
    /// <returns>The culture for a valid header, or null so the default culture applies.</returns>
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var language = httpContext.Request.Headers["X-Language"].ToString().Trim().ToLowerInvariant();
        var languages = httpContext.RequestServices.GetRequiredService<ISupportedLanguagesProvider>();

        return languages.IsSupported(language)
            ? Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(language))
            : Task.FromResult<ProviderCultureResult?>(null);
    }
}
