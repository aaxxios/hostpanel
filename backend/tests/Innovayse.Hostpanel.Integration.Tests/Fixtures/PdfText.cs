using UglyToad.PdfPig;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>Reads the text of a PDF response, for asserting on what a downloaded invoice says.</summary>
public static class PdfText
{
    /// <summary>
    /// The text of every page with whitespace removed, so line wrapping and glyph spacing cannot
    /// break a match. Compare against a value with its spaces removed too.
    /// </summary>
    /// <param name="response">A response whose body is a PDF.</param>
    /// <returns>The document's text without whitespace.</returns>
    public static async Task<string> CompactAsync(HttpResponseMessage response)
    {
        var bytes = await response.Content.ReadAsByteArrayAsync();
        using var document = PdfDocument.Open(bytes);
        return string.Concat(document.GetPages().SelectMany(p => p.Text).Where(c => !char.IsWhiteSpace(c)));
    }
}
