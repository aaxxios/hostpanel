using Innovayse.Hostpanel.SharedKernel.Domain;
using Innovayse.Hostpanel.SharedKernel.Domain.Interfaces;

namespace Innovayse.Hostpanel.Domain.Locations;

/// <summary>
/// A data-centre location shown on the storefront, with per-locale translations.
/// Stored in the <c>ServerLocations</c> table.
/// </summary>
public sealed class ServerLocation : BaseEntity, IAggregateRoot, ISortable
{
    /// <summary>Length of an ISO 3166 alpha-2 country code: matches the <c>ServerLocations.CountryCode</c> column.</summary>
    public const int CountryCodeLength = 2;

    /// <summary>Gets the ISO 3166 alpha-2 country code, upper case.</summary>
    public string CountryCode { get; private set; } = string.Empty;

    /// <summary>Gets the display order (ascending).</summary>
    public int SortOrder { get; private set; }

    /// <summary>Gets whether the location is currently shown on the storefront.</summary>
    public bool IsActive { get; private set; }

    /// <summary>Gets the UTC creation timestamp.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Backing field for translations.</summary>
    private readonly List<ServerLocationTranslation> _translations = [];

    /// <summary>Gets the per-locale translations.</summary>
    public IReadOnlyList<ServerLocationTranslation> Translations => _translations.AsReadOnly();

    /// <summary>EF Core parameterless constructor: do not call directly.</summary>
    private ServerLocation() { }

    /// <summary>
    /// Creates a new location with no translations yet.
    /// </summary>
    /// <param name="countryCode">ISO 3166 alpha-2 country code; stored upper case.</param>
    /// <param name="sortOrder">Display order (ascending).</param>
    /// <param name="isActive">Whether the location is shown on the storefront.</param>
    /// <returns>A new <see cref="ServerLocation"/>.</returns>
    /// <exception cref="DomainRuleException">The sort order is negative or the country code is not two letters.</exception>
    public static ServerLocation Create(string countryCode, int sortOrder, bool isActive)
    {
        EnsureSortOrder(sortOrder);
        var code = NormalizeCountryCode(countryCode);

        return new ServerLocation
        {
            CountryCode = code,
            SortOrder = sortOrder,
            IsActive = isActive,
        };
    }

    /// <summary>
    /// Updates every non-translation field.
    /// </summary>
    /// <param name="countryCode">ISO 3166 alpha-2 country code; stored upper case.</param>
    /// <param name="sortOrder">New display order.</param>
    /// <param name="isActive">New active state.</param>
    /// <exception cref="DomainRuleException">The sort order is negative or the country code is not two letters.</exception>
    public void Update(string countryCode, int sortOrder, bool isActive)
    {
        EnsureSortOrder(sortOrder);
        var code = NormalizeCountryCode(countryCode);

        CountryCode = code;
        SortOrder = sortOrder;
        IsActive = isActive;
    }

    /// <summary>Shows the location on the storefront.</summary>
    public void Activate() => IsActive = true;

    /// <summary>Hides the location from the storefront.</summary>
    public void Deactivate() => IsActive = false;

    /// <summary>Moves the location to a new position in the display order.</summary>
    /// <param name="sortOrder">The new display order (ascending).</param>
    /// <exception cref="DomainRuleException">The sort order is negative.</exception>
    public void SetSortOrder(int sortOrder)
    {
        EnsureSortOrder(sortOrder);
        SortOrder = sortOrder;
    }

    /// <summary>
    /// Adds or updates the translation for the given locale.
    /// </summary>
    /// <param name="locale">BCP-47 locale code.</param>
    /// <param name="name">The data-centre name in this locale.</param>
    /// <param name="city">The city in this locale, or <c>null</c>.</param>
    /// <param name="latencyNote">A short latency note in this locale, or <c>null</c>.</param>
    public void SetTranslation(string locale, string name, string? city, string? latencyNote)
    {
        var existing = _translations.FirstOrDefault(t => t.Locale == locale);
        if (existing is not null)
        {
            existing.Update(name, city, latencyNote);
        }
        else
        {
            _translations.Add(ServerLocationTranslation.Create(locale, name, city, latencyNote));
        }
    }

    /// <summary>
    /// Removes the translation for the given locale, if there is one.
    /// </summary>
    /// <param name="locale">BCP-47 locale code of the translation to remove.</param>
    public void RemoveTranslation(string locale)
    {
        var existing = _translations.FirstOrDefault(t => t.Locale == locale);
        if (existing is not null)
        {
            _translations.Remove(existing);
        }
    }

    /// <summary>
    /// Makes the location's translations exactly the given set: locales that are not in it are
    /// removed, the rest are added or updated in place.
    /// </summary>
    /// <param name="translations">The complete set of translations, keyed by BCP-47 locale code.</param>
    /// <exception cref="ArgumentNullException"><paramref name="translations"/> is null.</exception>
    public void ReplaceTranslations(IReadOnlyDictionary<string, ServerLocationTranslationContent> translations)
    {
        ArgumentNullException.ThrowIfNull(translations);

        _translations.RemoveAll(t => !translations.ContainsKey(t.Locale));
        foreach (var (locale, content) in translations)
        {
            SetTranslation(locale, content.Name, content.City, content.LatencyNote);
        }
    }

    /// <summary>Refuses a negative sort order.</summary>
    /// <param name="sortOrder">The display order to check.</param>
    /// <exception cref="DomainRuleException">The sort order is negative.</exception>
    private static void EnsureSortOrder(int sortOrder)
    {
        if (sortOrder < 0)
        {
            throw new DomainRuleException(ServerLocationErrors.SortOrderNegative, "A sort order cannot be negative.");
        }
    }

    /// <summary>Checks a country code is two ASCII letters and returns it upper case.</summary>
    /// <param name="countryCode">The code to check.</param>
    /// <returns>The code in upper case.</returns>
    /// <exception cref="DomainRuleException">The code is not two letters.</exception>
    private static string NormalizeCountryCode(string? countryCode)
    {
        var code = countryCode?.Trim() ?? string.Empty;
        if (code.Length != CountryCodeLength || !code.All(char.IsAsciiLetter))
        {
            throw new DomainRuleException(ServerLocationErrors.CountryCodeInvalid, "A country code is two letters.");
        }

        return code.ToUpperInvariant();
    }
}
