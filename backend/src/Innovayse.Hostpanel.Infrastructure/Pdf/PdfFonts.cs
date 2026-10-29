using System.Text;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Fonts;

namespace Innovayse.Hostpanel.Infrastructure.Pdf;

/// <summary>
/// The fonts invoice PDFs are set in, served to PDFsharp from the files embedded in this assembly.
/// </summary>
/// <remarks>
/// <para>
/// The production image is Alpine with no fonts installed, so PDFsharp must never look for system
/// fonts: every family it asks for resolves to an embedded file here, and nothing else.
/// </para>
/// <para>
/// DejaVu Sans covers Latin, Greek, Cyrillic, Armenian and Georgian letters in one face. It lacks
/// the dram sign (U+058F), the suffix of the Armenian dram, so Noto Sans Armenian stands behind it
/// for any character DejaVu has no glyph for. Both licences sit next to the files in
/// <c>Pdf/Fonts</c>.
/// </para>
/// <para>
/// PDFsharp's font resolver and licence are process-wide, so both are set once, by
/// <see cref="EnsureInitialized"/>, and nothing else in the process may set them.
/// </para>
/// </remarks>
internal static class PdfFonts
{
    /// <summary>The family every invoice is set in.</summary>
    public const string Primary = "DejaVu Sans";

    /// <summary>The family for characters <see cref="Primary"/> has no glyph for.</summary>
    public const string Fallback = "Noto Sans Armenian";

    /// <summary>The size the faces are opened at to ask which glyphs they have; any size answers alike.</summary>
    private const double ProbeSize = 10;

    /// <summary>Sets PDFsharp up the first time it is read; the value itself means nothing.</summary>
    private static readonly Lazy<bool> Initialized = new(() =>
    {
        // 6.5 onwards refuses to render until the AGPL licence is accepted in code.
        PdfSharpLicense.SetAgplLicense();
        GlobalFontSettings.FontResolver = new EmbeddedFontResolver();
        return true;
    });

    /// <summary>Accepts PDFsharp's licence and installs the embedded fonts, once per process.</summary>
    public static void EnsureInitialized() => _ = Initialized.Value;

    /// <summary>
    /// Splits text into runs, each set in <see cref="Primary"/> or, for characters only the
    /// fallback has, in <see cref="Fallback"/>.
    /// </summary>
    /// <param name="text">The text to split.</param>
    /// <param name="bold">Whether it is set in bold.</param>
    /// <returns>The runs in order; together they spell <paramref name="text"/>.</returns>
    /// <remarks>
    /// The faces are opened on every call rather than kept in static fields: renders run side by
    /// side on different threads, and an <see cref="XFont"/> is not safe to share between them.
    /// </remarks>
    public static IEnumerable<(string Text, bool UseFallback)> Runs(string text, bool bold)
    {
        EnsureInitialized();
        var style = bold ? XFontStyleEx.Bold : XFontStyleEx.Regular;
        var primary = new XFont(Primary, ProbeSize, style);
        var fallback = new XFont(Fallback, ProbeSize, style);

        var run = new StringBuilder();
        var runIsFallback = false;
        foreach (var rune in text.EnumerateRunes())
        {
            var useFallback = GlyphHelper.GlyphIndexFromCodePoint(rune.Value, primary) == 0
                && GlyphHelper.GlyphIndexFromCodePoint(rune.Value, fallback) != 0;
            if (useFallback != runIsFallback && run.Length > 0)
            {
                yield return (run.ToString(), runIsFallback);
                run.Clear();
            }

            runIsFallback = useFallback;
            run.Append(rune.ToString());
        }

        if (run.Length > 0)
        {
            yield return (run.ToString(), runIsFallback);
        }
    }
}
