using System.Net;
using System.Text.RegularExpressions;

namespace Innovayse.Hostpanel.Application.Support.Common;

/// <summary>Turns the opening of an article's HTML body into the short plain-text lead a listing shows.</summary>
public static partial class KbArticleExcerpt
{
    /// <summary>Longest excerpt, in characters.</summary>
    public const int MaxLength = 140;

    /// <summary>
    /// How many characters of the stored body the database hands over for building one excerpt. Several
    /// times <see cref="MaxLength"/>, because the markup at the start of a body (tags, attributes) uses up
    /// characters that never reach the text; a body whose first 560 characters are almost all markup gets a
    /// shorter excerpt, which is acceptable for a preview.
    /// </summary>
    public const int SourceLength = 4 * MaxLength;

    /// <summary>
    /// Builds the excerpt from the opening of a body: tags removed (a tag cut off by the end of the prefix
    /// as well), HTML entities decoded, every run of whitespace collapsed to one space, trimmed and cut to
    /// <see cref="MaxLength"/> characters at a word boundary.
    /// </summary>
    /// <remarks>
    /// The cut falls on the last space when that space is in the second half of the excerpt; a text with
    /// no such space (one very long word) is cut at <see cref="MaxLength"/> instead of being reduced to
    /// almost nothing.
    /// </remarks>
    /// <param name="htmlLead">The first <see cref="SourceLength"/> characters of the body as stored.</param>
    /// <returns>The plain-text excerpt; empty for an empty body.</returns>
    public static string From(string htmlLead)
    {
        ArgumentNullException.ThrowIfNull(htmlLead);

        var withoutTags = OpenTag().Replace(Tag().Replace(htmlLead, " "), " ");
        var text = Whitespace().Replace(WebUtility.HtmlDecode(withoutTags), " ").Trim();
        if (text.Length <= MaxLength)
        {
            return text;
        }

        var cut = text.LastIndexOf(' ', MaxLength);
        return cut > MaxLength / 2 ? text[..cut] : text[..MaxLength];
    }

    /// <summary>One HTML tag, opening or closing, with its attributes.</summary>
    /// <returns>The compiled pattern.</returns>
    [GeneratedRegex("<[^>]*>", RegexOptions.CultureInvariant)]
    private static partial Regex Tag();

    /// <summary>A tag that starts but is not closed, because the prefix ended inside it.</summary>
    /// <returns>The compiled pattern.</returns>
    [GeneratedRegex("<[^>]*$", RegexOptions.CultureInvariant)]
    private static partial Regex OpenTag();

    /// <summary>A run of whitespace characters.</summary>
    /// <returns>The compiled pattern.</returns>
    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex Whitespace();
}
