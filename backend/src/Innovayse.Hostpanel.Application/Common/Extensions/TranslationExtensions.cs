namespace Innovayse.Hostpanel.Application.Common.Extensions;

/// <summary>Picks the best translation of a piece of content for the language a caller asked for.</summary>
public static class TranslationExtensions
{
    /// <summary>
    /// Resolves a translation by locale: the requested one, else the fallback (default) locale, else the
    /// first of the rest in locale order. The last step is ordered rather than "whichever is stored first",
    /// because storage order is not guaranteed and the same request must always answer the same text.
    /// </summary>
    /// <typeparam name="T">The translation entity.</typeparam>
    /// <param name="translations">Every translation stored for one piece of content.</param>
    /// <param name="localeOf">Reads the locale code of a translation.</param>
    /// <param name="requested">The locale the caller asked for.</param>
    /// <param name="fallback">The panel's default locale.</param>
    /// <returns>The chosen translation, or <see langword="null"/> when none is stored.</returns>
    public static T? ForLocale<T>(
        this IEnumerable<T> translations, Func<T, string> localeOf, string requested, string fallback)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(translations);
        ArgumentNullException.ThrowIfNull(localeOf);

        var all = translations.ToList();

        return all.FirstOrDefault(t => string.Equals(localeOf(t), requested, StringComparison.OrdinalIgnoreCase))
            ?? all.FirstOrDefault(t => string.Equals(localeOf(t), fallback, StringComparison.OrdinalIgnoreCase))
            ?? all.OrderBy(localeOf, StringComparer.Ordinal).FirstOrDefault();
    }
}
