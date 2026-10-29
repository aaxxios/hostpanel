using System.Reflection;
using PdfSharp.Fonts;

namespace Innovayse.Hostpanel.Infrastructure.Pdf;

/// <summary>Resolves the two invoice PDF families, in both weights, to the font files embedded in this assembly.</summary>
/// <remarks>
/// Installed once per process by <see cref="PdfFonts.EnsureInitialized"/>. It never falls through to
/// system fonts, because the production image has none.
/// </remarks>
internal sealed class EmbeddedFontResolver : IFontResolver
{
    /// <summary>Prefix of the manifest resource names the csproj gives the embedded fonts.</summary>
    private const string ResourcePrefix = "Innovayse.Hostpanel.Infrastructure.Pdf.Fonts.";

    /// <inheritdoc/>
    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic) => familyName switch
    {
        PdfFonts.Fallback => new FontResolverInfo(isBold ? "NotoSansArmenian-Bold" : "NotoSansArmenian-Regular"),

        // Anything else, including a family MigraDoc asks for by default, gets the main face.
        _ => new FontResolverInfo(isBold ? "DejaVuSans-Bold" : "DejaVuSans"),
    };

    /// <inheritdoc/>
    public byte[]? GetFont(string faceName) => Load(faceName + ".ttf");

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
}
