using System.Globalization;

namespace Innovayse.Hostpanel.Application.Common;

/// <summary>
/// Builds the pattern for a case-insensitive "contains" match through <c>EF.Functions.Like</c>.
/// A search term typed by a person is data, not a pattern: its <c>%</c>, <c>_</c> and escape
/// characters are escaped here so they match themselves instead of widening the search.
/// </summary>
public static class LikePattern
{
    /// <summary>The character that escapes a wildcard inside the pattern; pass it as the escape argument of <c>Like</c>.</summary>
    public const string EscapeCharacter = "\\";

    /// <summary>
    /// Returns the pattern that matches any value containing the term. The term is lower-cased, so the
    /// column must be lower-cased too: <c>EF.Functions.Like(column.ToLower(), pattern, EscapeCharacter)</c>.
    /// </summary>
    /// <param name="term">What the person typed.</param>
    /// <returns>The lower-cased, escaped term wrapped in <c>%</c> wildcards.</returns>
    public static string Contains(string term)
    {
        ArgumentNullException.ThrowIfNull(term);

        var escaped = term
            .ToLower(CultureInfo.InvariantCulture)
            .Replace(EscapeCharacter, EscapeCharacter + EscapeCharacter, StringComparison.Ordinal)
            .Replace("%", EscapeCharacter + "%", StringComparison.Ordinal)
            .Replace("_", EscapeCharacter + "_", StringComparison.Ordinal);

        return $"%{escaped}%";
    }
}
