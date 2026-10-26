using System.Reflection;
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

    /// <summary>Prefix of the manifest resource names the csproj gives the embedded fonts.</summary>
    private const string ResourcePrefix = "Innovayse.Hostpanel.Infrastructure.Pdf.Fonts.";

    /// <summary>Sets PDFsharp up the first time it is read; the value itself means nothing.</summary>
    private static readonly Lazy<bool> Initialized = new(() =>
    {
        // 6.5 onwards refuses to render until the AGPL licence is accepted in code.
        PdfSharpLicense.SetAgplLicense();
        GlobalFontSettings.FontResolver = new EmbeddedFontResolver();
        return true;
    });

    /// <summary>The main face at an arbitrary size, for asking which glyphs it has.</summary>
    private static readonly Lazy<XFont> PrimaryRegular = new(() => new XFont(Primary, 10));

    /// <summary>The bold main face, for asking which glyphs it has.</summary>
    private static readonly Lazy<XFont> PrimaryBold = new(() => new XFont(Primary, 10, XFontStyleEx.Bold));

    /// <summary>The fallback face, for asking which glyphs it has.</summary>
    private static readonly Lazy<XFont> FallbackRegular = new(() => new XFont(Fallback, 10));

    /// <summary>The bold fallback face, for asking which glyphs it has.</summary>
    private static readonly Lazy<XFont> FallbackBold = new(() => new XFont(Fallback, 10, XFontStyleEx.Bold));

    /// <summary>Accepts PDFsharp's licence and installs the embedded fonts, once per process.</summary>
    public static void EnsureInitialized() => _ = Initialized.Value;

    /// <summary>
    /// Splits text into runs, each set in <see cref="Primary"/> or, for characters only the
    /// fallback has, in <see cref="Fallback"/>.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="bold">Whether it is set in bold.</param>
    /// <returns>The runs in order; together they spell <paramref name="text"/>.</returns>
    public static IEnumerable<(string Text, bool UseFallback)> Runs(string text, bool bold)
    {
        EnsureInitialized();
        var primary = bold ? PrimaryBold.Value : PrimaryRegular.Value;
        var fallback = bold ? FallbackBold.Value : FallbackRegular.Value;

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

    /// <summary>Reads one embedded font.</summary>
    /// <param name="fileName">The font's file name under <c>Pdf/Fonts</c>.</param>
    /// <returns>The font file's bytes.</returns>
    /// <exception cref="InvalidOperationException">The font is not embedded.</exception>
    private static byte[] Load(string fileName)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourcePrefix + fileName)
            ?? throw new InvalidOperationException($"Embedded font '{fileName}' is missing.");
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }

    /// <summary>Resolves the two families, in both weights, to the embedded files.</summary>
    private sealed class EmbeddedFontResolver : IFontResolver
    {
        /// <inheritdoc/>
        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic) => familyName switch
        {
            Fallback => new FontResolverInfo(isBold ? "NotoSansArmenian-Bold" : "NotoSansArmenian-Regular"),

            // Anything else, including a family MigraDoc asks for by default, gets the main face.
            _ => new FontResolverInfo(isBold ? "DejaVuSans-Bold" : "DejaVuSans"),
        };

        /// <inheritdoc/>
        public byte[]? GetFont(string faceName) => Load(faceName + ".ttf");
    }
}
