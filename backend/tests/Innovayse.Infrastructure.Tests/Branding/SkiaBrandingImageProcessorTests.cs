namespace Innovayse.Infrastructure.Tests.Branding;

using FluentAssertions;
using Innovayse.Application.Admin.Common;
using Innovayse.Infrastructure.Branding;
using Innovayse.Infrastructure.Branding.Options;
using Microsoft.Extensions.Options;
using SkiaSharp;
using Xunit;

/// <summary>
/// Unit tests for the branding upload renderer.
/// </summary>
/// <remarks>
/// The inputs are drawn in memory with SkiaSharp itself, so no fixture files are needed. What is
/// worth checking is the contract the panel relies on: every output is a PNG this class encoded,
/// at the size the favicon set promises, and every refusal happens before pixels are decoded.
/// </remarks>
public sealed class SkiaBrandingImageProcessorTests
{
    /// <summary>The PNG file signature.</summary>
    private static readonly byte[] _pngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    /// <summary>Builds a processor with default options, or the given ones.</summary>
    /// <param name="options">Options to use instead of the defaults.</param>
    /// <returns>The processor under test.</returns>
    private static SkiaBrandingImageProcessor Create(BrandingOptions? options = null) =>
        new(Options.Create(options ?? new BrandingOptions()));

    /// <summary>Draws a solid image with a contrasting square in the middle and encodes it.</summary>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="format">Container format to encode.</param>
    /// <param name="background">Background colour (top-left pixel).</param>
    /// <returns>The encoded bytes.</returns>
    private static byte[] MakeImage(
        int width,
        int height,
        SKEncodedImageFormat format = SKEncodedImageFormat.Png,
        SKColor? background = null)
    {
        using var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(background ?? new SKColor(0x10, 0x40, 0xC0));
            using var paint = new SKPaint { Color = SKColors.White };
            canvas.DrawRect(width / 4f, height / 4f, width / 2f, height / 2f, paint);
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, 90);
        return data.ToArray();
    }

    /// <summary>Decodes rendition bytes back into a bitmap for inspection.</summary>
    /// <param name="png">PNG bytes.</param>
    /// <returns>The decoded bitmap.</returns>
    private static SKBitmap Read(byte[] png) => SKBitmap.Decode(png);

    [Fact]
    public async Task Favicon_ProducesTheFullSquareSetAsPng()
    {
        var renditions = await Create().RenderAsync(
            new BrandingSource(MakeImage(300, 120), "logo.png"), BrandingKind.Favicon, default);

        renditions.Select(r => r.FileName).Should().BeEquivalentTo(
        [
            "favicon.png", "favicon-16x16.png", "favicon-32x32.png", "apple-touch-icon.png",
            "icon-192.png", "icon-512.png", "icon-maskable-192.png", "icon-maskable-512.png",
        ]);

        foreach (var rendition in renditions)
        {
            rendition.ContentType.Should().Be("image/png");
            rendition.Content.Take(8).Should().Equal(_pngSignature);

            using var bitmap = Read(rendition.Content);
            bitmap.Width.Should().Be(rendition.PixelSize);
            bitmap.Height.Should().Be(rendition.PixelSize, "every icon is square");
        }
    }

    [Fact]
    public async Task Favicon_PadsAWideImageWithTransparency()
    {
        var renditions = await Create().RenderAsync(
            new BrandingSource(MakeImage(400, 100), "wide.png"), BrandingKind.Favicon, default);

        using var icon = Read(renditions.Single(r => r.FileName == "icon-512.png").Content);

        icon.GetPixel(256, 0).Alpha.Should().Be(0, "the band above a wide logo is padding");
        icon.GetPixel(256, 256).Alpha.Should().Be(255, "the artwork itself is opaque");
    }

    [Fact]
    public async Task Maskable_FillsTheCanvasWithTheSourceCornerColour()
    {
        var blue = new SKColor(0x10, 0x40, 0xC0);
        var renditions = await Create().RenderAsync(
            new BrandingSource(MakeImage(200, 200, background: blue), "mark.png"), BrandingKind.Favicon, default);

        using var maskable = Read(renditions.Single(r => r.FileName == "icon-maskable-512.png").Content);

        maskable.GetPixel(0, 0).Should().Be(blue);
        maskable.GetPixel(511, 511).Alpha.Should().Be(255, "a maskable icon may not be transparent");
    }

    [Fact]
    public async Task Maskable_FallsBackToWhiteForATransparentCorner()
    {
        var renditions = await Create().RenderAsync(
            new BrandingSource(MakeImage(200, 200, background: SKColors.Transparent), "mark.png"),
            BrandingKind.Favicon,
            default);

        using var maskable = Read(renditions.Single(r => r.FileName == "icon-maskable-192.png").Content);

        maskable.GetPixel(0, 0).Should().Be(SKColors.White);
    }

    [Theory]
    [InlineData("favicon-16x16.png", 205, 240)]
    [InlineData("favicon-32x32.png", 205, 240)]
    // At 180px each output pixel covers 5.7 source columns, which hold either one line or
    // none, so even an exact area average ranges from 210 to 255. A 4-tap cubic filter gave
    // 29 to 255 here; the bounds allow the area average and refuse that.
    [InlineData("apple-touch-icon.png", 185, 255)]
    public async Task Favicon_DownscalesFineDetailWithoutAliasing(string file, int min, int max)
    {
        // 1px black lines every 8px on white: the true average brightness is 255 * 7/8 = 223.
        // A filter that samples only a few source pixels lands far from that, or in moire.
        using var bitmap = new SKBitmap(new SKImageInfo(1024, 1024, SKColorType.Rgba8888, SKAlphaType.Premul));
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.White);
            using var paint = new SKPaint { Color = SKColors.Black };
            for (var x = 0; x < 1024; x += 8)
            {
                canvas.DrawRect(x, 0, 1, 1024, paint);
            }
        }

        using var source = SKImage.FromBitmap(bitmap);
        using var png = source.Encode(SKEncodedImageFormat.Png, 100);

        var renditions = await Create().RenderAsync(
            new BrandingSource(png.ToArray(), "stripes.png"), BrandingKind.Favicon, default);

        using var icon = Read(renditions.Single(r => r.FileName == file).Content);
        var values = Enumerable.Range(0, icon.Width).Select(x => (int)icon.GetPixel(x, icon.Height / 2).Red).ToList();

        values.Should().OnlyContain(v => v >= min && v <= max, "fine detail should average out, not alias");
    }

    [Fact]
    public async Task Logo_IsDownscaledToTheEdgeLimitKeepingAspectRatio()
    {
        var renditions = await Create(new BrandingOptions { MaxLogoEdge = 256 }).RenderAsync(
            new BrandingSource(MakeImage(1000, 500), "logo.png"), BrandingKind.Logo, default);

        var logo = renditions.Should().ContainSingle().Subject;
        logo.FileName.Should().Be("logo.png");

        using var bitmap = Read(logo.Content);
        bitmap.Width.Should().Be(256);
        bitmap.Height.Should().Be(128);
    }

    [Fact]
    public async Task Logo_IsNeverEnlarged()
    {
        var renditions = await Create().RenderAsync(
            new BrandingSource(MakeImage(300, 100), "small.png"), BrandingKind.Logo, default);

        using var bitmap = Read(renditions.Single().Content);
        bitmap.Width.Should().Be(300);
        bitmap.Height.Should().Be(100);
    }

    [Theory]
    [InlineData(SKEncodedImageFormat.Jpeg)]
    [InlineData(SKEncodedImageFormat.Webp)]
    public async Task AcceptedFormats_AreReEncodedAsPng(SKEncodedImageFormat format)
    {
        var renditions = await Create().RenderAsync(
            new BrandingSource(MakeImage(64, 64, format), "upload"), BrandingKind.Logo, default);

        renditions.Single().Content.Take(8).Should().Equal(_pngSignature);
    }

    [Fact]
    public async Task Svg_IsRefusedWithAMessageSayingWhatToDo()
    {
        var svg = "<?xml version=\"1.0\"?><svg xmlns=\"http://www.w3.org/2000/svg\"><script>alert(1)</script></svg>"u8.ToArray();

        var act = () => Create().RenderAsync(new BrandingSource(svg, "logo.svg"), BrandingKind.Logo, default);

        (await act.Should().ThrowAsync<InvalidBrandingImageException>())
            .Which.Reason.Should().Contain("convert to PNG");
    }

    [Fact]
    public async Task UnrecognisedBytes_AreRefused()
    {
        var act = () => Create().RenderAsync(
            new BrandingSource("not an image at all"u8.ToArray(), "x.png"), BrandingKind.Logo, default);

        await act.Should().ThrowAsync<InvalidBrandingImageException>();
    }

    [Fact]
    public async Task EmptyUpload_IsRefused()
    {
        var act = () => Create().RenderAsync(new BrandingSource([], "x.png"), BrandingKind.Logo, default);

        await act.Should().ThrowAsync<InvalidBrandingImageException>();
    }

    [Fact]
    public async Task OversizedEdge_IsRefusedFromTheHeader()
    {
        var act = () => Create(new BrandingOptions { MaxSourceEdge = 100 }).RenderAsync(
            new BrandingSource(MakeImage(200, 50), "big.png"), BrandingKind.Logo, default);

        await act.Should().ThrowAsync<BrandingImageTooLargeException>();
    }

    [Fact]
    public async Task OversizedArea_IsRefusedEvenWhenBothEdgesFit()
    {
        var act = () => Create(new BrandingOptions { MaxSourceEdge = 1000, MaxSourcePixels = 10_000 })
            .RenderAsync(new BrandingSource(MakeImage(200, 200), "big.png"), BrandingKind.Logo, default);

        await act.Should().ThrowAsync<BrandingImageTooLargeException>();
    }

    [Fact]
    public async Task OversizedUpload_IsRefusedByByteCount()
    {
        var act = () => Create(new BrandingOptions { MaxBytes = 10 }).RenderAsync(
            new BrandingSource(MakeImage(64, 64), "x.png"), BrandingKind.Logo, default);

        await act.Should().ThrowAsync<BrandingImageTooLargeException>();
    }

    [Fact]
    public async Task TruncatedFile_IsRefusedRatherThanStoredHalfDecoded()
    {
        var png = MakeImage(256, 256);
        var truncated = png.Take(png.Length / 2).ToArray();

        var act = () => Create().RenderAsync(new BrandingSource(truncated, "cut.png"), BrandingKind.Logo, default);

        await act.Should().ThrowAsync<InvalidBrandingImageException>();
    }

    [Fact]
    public async Task Metadata_IsDroppedByReEncoding()
    {
        // A trailing payload after IEND is the classic "image and archive at once" polyglot.
        var payload = "PK\u0003\u0004 not-an-image-but-a-zip"u8.ToArray();
        var polyglot = MakeImage(64, 64).Concat(payload).ToArray();

        var renditions = await Create().RenderAsync(
            new BrandingSource(polyglot, "poly.png"), BrandingKind.Logo, default);

        var stored = renditions.Single().Content;
        System.Text.Encoding.ASCII.GetString(stored).Should().NotContain("not-an-image-but-a-zip");
    }
}
