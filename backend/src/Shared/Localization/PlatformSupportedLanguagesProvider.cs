using Innovayse.Hostpanel.Shared.Constants;
using Innovayse.Hostpanel.Shared.Contracts.Interfaces;

namespace Innovayse.Hostpanel.Shared.Localization;

/// <summary>
/// <see cref="ISupportedLanguagesProvider"/> over <see cref="SupportedLanguages"/>. The house reads a
/// database-backed platform setting; this service has no platform-settings table, so it delegates to
/// the static single source of truth.
/// </summary>
public sealed class PlatformSupportedLanguagesProvider : ISupportedLanguagesProvider
{
    /// <summary>The static list, copied once so callers cannot mutate the shared set through it.</summary>
    private static readonly IReadOnlyList<string> AllLanguages = [.. SupportedLanguages.All];

    /// <inheritdoc />
    public IReadOnlyList<string> Languages => AllLanguages;

    /// <inheritdoc />
    public string Default => SupportedLanguages.Default;

    /// <inheritdoc />
    public bool IsSupported(string language)
    {
        return SupportedLanguages.IsSupported(language);
    }
}
