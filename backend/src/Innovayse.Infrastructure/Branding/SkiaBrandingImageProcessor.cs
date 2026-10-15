namespace Innovayse.Infrastructure.Branding;

using Innovayse.Application.Admin.Common;
using Innovayse.Application.Admin.Interfaces;
using Innovayse.Infrastructure.Branding.Options;
using Microsoft.Extensions.Options;
using SkiaSharp;

/// <summary>
/// Renders branding uploads with SkiaSharp, deciding the format from the bytes themselves.
/// </summary>
/// <remarks>
/// <para>
/// <b>Every stored file is a PNG this class encoded.</b> Nothing an operator uploads is written
/// back out as it arrived, and that is the security property the class exists for rather than an
/// incidental choice: re-encoding drops every ancillary chunk — EXIF with the photographer's
/// coordinates, colour profiles, and the trailing payload that makes one file a valid image
/// <i>and</i> a valid archive or script at the same time.
/// </para>
/// <para>
/// <b>SVG is refused, not stored.</b> Storing the vector untouched is exactly the thing not to
/// do: an SVG served from the same origin as the panel is a script tag with a picture around it —
/// it carries <c>&lt;script&gt;</c> and event handlers, and a browser runs them, which turns "an
/// admin uploads a logo" into stored XSS against every visitor. So the upload is refused with a
/// readable message and the operator converts to PNG once, rather than the product growing a
/// sanitiser it would have to keep ahead of.
/// </para>
/// <para>
/// The format is decided by <see cref="SKCodec"/>, which reads the container's own header. The
/// multipart part's <c>Content-Type</c> and the filename's extension are both written by the
/// client and are therefore claims, not evidence.
/// </para>
/// <para>
/// This replaced an ImageSharp implementation with the same behaviour: SixLabors.ImageSharp 2.x
/// stopped receiving fixes and 3.x is not under a licence this repository can take. See
/// <c>backend/Directory.Packages.props</c>.
/// </para>
/// </remarks>
/// <param name="options">Ceilings and paths for branding uploads.</param>
public sealed class SkiaBrandingImageProcessor(IOptions<BrandingOptions> options)
    : IBrandingImageProcessor
{
    /// <summary>
    /// The square icon set generated from a favicon upload: the file name each rendition is
    /// stored under, its edge in pixels, and what links it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The names are the workspace's, not this feature's -- every product's static root carries
    /// the same eight file names, and `innovayse-workspace/brand/README.md` is where that list
    /// is written down. An upload has to produce exactly those names, because the pages that
    /// link them are the same pages that link the committed defaults.
    /// </para>
    /// <para>
    /// 16 and 32 are the browser tab and the bookmark bar; 180 is the current iOS home-screen
    /// size and the only one Safari reads; 192 and 512 are the two the web app manifest
    /// specification requires an installable PWA to declare, once as <c>any</c> and again as
    /// <c>maskable</c>.
    /// </para>
    /// </remarks>
    private static readonly (string File, int Size, BrandingRole Role)[] _faviconSet =
    [
        ("favicon-16x16.png", 16, BrandingRole.Icon),
        ("favicon-32x32.png", 32, BrandingRole.Icon),
        ("apple-touch-icon.png", 180, BrandingRole.AppleTouch),
        ("icon-192.png", 192, BrandingRole.Android),
        ("icon-512.png", 512, BrandingRole.Android),
        ("icon-maskable-192.png", 192, BrandingRole.Maskable),
        ("icon-maskable-512.png", 512, BrandingRole.Maskable),
    ];

    /// <summary>
    /// Formats this deployment accepts.
    /// </summary>
    /// <remarks>
    /// An allow-list rather than "whatever the codec can decode". Skia also reads ICO, WBMP and
    /// several camera raw formats, which no browser renders as a favicon — accepting them would
    /// let an operator upload a file that stores fine and then fails to display, with nothing
    /// explaining why.
    /// </remarks>
    private static readonly HashSet<SKEncodedImageFormat> _acceptedFormats =
    [
        SKEncodedImageFormat.Png,
        SKEncodedImageFormat.Jpeg,
        SKEncodedImageFormat.Gif,
        SKEncodedImageFormat.Webp,
        SKEncodedImageFormat.Bmp,
    ];

    /// <summary>
    /// Resampling for every resize: trilinear, i.e. bilinear between mipmap levels.
    /// </summary>
    /// <remarks>
    /// Not bicubic. Skia's cubic filters are fixed 4-tap kernels that ignore mipmaps, so a
    /// 1024px mark reduced to a 16px favicon samples a handful of source pixels and aliases:
    /// fine stripes came out as the wrong overall brightness and moire. Mipmapped sampling
    /// averages the whole footprint, which is what a large downscale needs.
    /// </remarks>
    private static readonly SKSamplingOptions _sampling = new(SKFilterMode.Linear, SKMipmapMode.Linear);

    /// <summary>Resolved ceilings and paths.</summary>
    private readonly BrandingOptions _options = options.Value;

    /// <inheritdoc />
    public Task<IReadOnlyList<BrandingRendition>> RenderAsync(
        BrandingSource source,
        BrandingKind kind,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (source.Content.Length == 0)
        {
            throw new InvalidBrandingImageException("Upload was empty.");
        }

        if (source.Content.Length > _options.MaxBytes)
        {
            throw new BrandingImageTooLargeException(
                $"Upload was {source.Content.Length} bytes; ceiling is {_options.MaxBytes}.");
        }

        using var image = Decode(source.Content);

        IReadOnlyList<BrandingRendition> renditions = kind == BrandingKind.Favicon
            ? RenderFaviconSet(image)
            : RenderLogo(image);

        return Task.FromResult(renditions);
    }

    /// <summary>
    /// Turns uploaded bytes into an image, or refuses them.
    /// </summary>
    /// <param name="content">The uploaded bytes.</param>
    /// <returns>The decoded image, unpremultiplied RGBA; the caller owns it.</returns>
    /// <exception cref="InvalidBrandingImageException">Not a format this accepts.</exception>
    /// <exception cref="BrandingImageTooLargeException">Declares more pixels than allowed.</exception>
    private SKBitmap Decode(byte[] content)
    {
        // Named before anything else, because it is the one refusal an operator will hit by
        // accident: the admin panel used to accept SVG, so the message has to say what to do.
        if (LooksLikeSvg(content))
        {
            throw new InvalidBrandingImageException(
                "Upload is SVG. Vectors are not stored: convert to PNG before uploading.");
        }

        using var stream = new SKMemoryStream(content);

        // Creating the codec reads the header only. This is the decompression-bomb check, and it
        // has to happen before the decode below: a highly compressible PNG a few hundred
        // kilobytes in size can declare a 100 000-pixel side and become forty gigabytes once its
        // pixels are materialised, so a byte ceiling alone lets one upload exhaust the container.
        using var codec = SKCodec.Create(stream)
            ?? throw new InvalidBrandingImageException("No decoder recognised the upload.");

        if (!_acceptedFormats.Contains(codec.EncodedFormat))
        {
            throw new InvalidBrandingImageException(
                $"Format '{codec.EncodedFormat}' is not accepted here.");
        }

        var width = codec.Info.Width;
        var height = codec.Info.Height;

        if (width <= 0 || height <= 0)
        {
            throw new InvalidBrandingImageException("Upload declares an empty image.");
        }

        if (width > _options.MaxSourceEdge || height > _options.MaxSourceEdge)
        {
            throw new BrandingImageTooLargeException(
                $"Source declares {width}x{height}; ceiling is {_options.MaxSourceEdge} per edge.");
        }

        // Area, not just edges. The buffer below is width * height * 4 bytes, so a shape that
        // clears both edge limits can still be the allocation that ends the process.
        var pixels = (long)width * height;
        if (pixels > _options.MaxSourcePixels)
        {
            throw new BrandingImageTooLargeException(
                $"Source declares {pixels} pixels; ceiling is {_options.MaxSourcePixels}.");
        }

        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        var bitmap = new SKBitmap(info);

        // GetPixels decodes the first frame only, which is what a logo or icon from an animated
        // GIF or WebP should be. IncompleteInput is a truncated file whose top rows decoded:
        // refused like any other broken upload rather than stored half grey.
        var result = codec.GetPixels(info, bitmap.GetPixels());
        if (result != SKCodecResult.Success)
        {
            bitmap.Dispose();
            throw new InvalidBrandingImageException(
                $"Header parsed but decoding failed: {result}.");
        }

        return bitmap;
    }

    /// <summary>Renders the full square icon set from a favicon upload.</summary>
    /// <param name="source">The decoded upload.</param>
    /// <returns>The primary rendition plus every size in the set.</returns>
    private static List<BrandingRendition> RenderFaviconSet(SKBitmap source)
    {
        var renditions = new List<BrandingRendition>(_faviconSet.Length + 1);

        // The primary is the largest ordinary icon rather than the upload as it arrived: it is
        // what a browser with no size preference picks, and it has to be square like the rest.
        // Named favicon.png, not favicon.ico -- this process encodes PNG, and every surface that
        // links the uploaded set links it by this name.
        var largest = _faviconSet.Where(f => f.Role != BrandingRole.Maskable).Max(f => f.Size);
        renditions.Add(new BrandingRendition(
            "favicon.png", EncodeSquare(source, largest), "image/png", largest, BrandingRole.Primary));

        foreach (var (file, size, role) in _faviconSet)
        {
            var bytes = role == BrandingRole.Maskable
                ? EncodeMaskable(source, size)
                : EncodeSquare(source, size);

            renditions.Add(new BrandingRendition(file, bytes, "image/png", size, role));
        }

        return renditions;
    }

    /// <summary>Renders the single stored logo, downscaled to fit if needed.</summary>
    /// <param name="source">The decoded upload.</param>
    /// <returns>A one-element list holding the primary rendition.</returns>
    private List<BrandingRendition> RenderLogo(SKBitmap source)
    {
        // Downscale only: a 512px mark handed to a 1024px box is stored at 512, not enlarged
        // into something blurrier and several times the bytes.
        var longest = Math.Max(source.Width, source.Height);
        if (longest <= _options.MaxLogoEdge)
        {
            return
            [
                new BrandingRendition("logo.png", Encode(source), "image/png", null, BrandingRole.Primary),
            ];
        }

        // Fit inside the box, keeping the aspect ratio.
        var scale = (double)_options.MaxLogoEdge / longest;
        var width = Math.Max(1, (int)Math.Round(source.Width * scale));
        var height = Math.Max(1, (int)Math.Round(source.Height * scale));

        using var logo = NewCanvasBitmap(width, height);
        using (var canvas = new SKCanvas(logo))
        {
            DrawFitted(canvas, source, new SKRect(0, 0, width, height));
        }

        return
        [
            new BrandingRendition("logo.png", Encode(logo), "image/png", null, BrandingRole.Primary),
        ];
    }

    /// <summary>
    /// Draws the source centred on a transparent square of the given edge, preserving aspect
    /// ratio, and encodes it as PNG.
    /// </summary>
    /// <remarks>
    /// Padded rather than stretched. An icon is square by definition, and a wide logo squashed
    /// into 512&#215;512 is the failure operators notice immediately. The padding is transparent,
    /// so a logo on a dark header does not gain a white box.
    /// </remarks>
    /// <param name="source">The decoded upload.</param>
    /// <param name="edge">Edge length of the output square, in pixels.</param>
    /// <returns>PNG bytes.</returns>
    private static byte[] EncodeSquare(SKBitmap source, int edge)
    {
        using var square = NewCanvasBitmap(edge, edge);
        using (var canvas = new SKCanvas(square))
        {
            canvas.Clear(SKColors.Transparent);
            DrawFitted(canvas, source, new SKRect(0, 0, edge, edge));
        }

        return Encode(square);
    }

    /// <summary>
    /// Renders the maskable variant: the upload held at 70% on a full-bleed square.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Android crops an adaptive icon to whatever silhouette the launcher chooses and
    /// guarantees only the inner 80%. The ordinary rendition cannot be reused: its transparent
    /// margin would be cropped into a smaller, lopsided square. So the canvas is filled edge to
    /// edge and the artwork sits well inside the safe circle -- the same 70% the workspace's
    /// hand-drawn maskable marks use, in `brand/maskable/`.
    /// </para>
    /// <para>
    /// The field is taken from the upload's own top-left pixel rather than a colour chosen
    /// here. An operator's mark usually sits on its own tile, and sampling it keeps the padded
    /// area continuous with the artwork instead of introducing a colour the brand never had. A
    /// fully transparent corner falls back to white, because a maskable icon may not be
    /// transparent: the launcher would fill the crop with whatever it liked. A partly
    /// transparent corner is used fully opaque, for the same reason.
    /// </para>
    /// </remarks>
    /// <param name="source">The decoded upload.</param>
    /// <param name="edge">Edge length of the output square, in pixels.</param>
    /// <returns>PNG bytes.</returns>
    private static byte[] EncodeMaskable(SKBitmap source, int edge)
    {
        var corner = source.GetPixel(0, 0);
        var field = corner.Alpha == 0 ? SKColors.White : corner.WithAlpha(255);
        var inner = (int)Math.Round(edge * 0.7);
        var offset = (edge - inner) / 2f;

        using var bitmap = NewCanvasBitmap(edge, edge);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(field);
            DrawFitted(canvas, source, new SKRect(offset, offset, offset + inner, offset + inner));
        }

        return Encode(bitmap);
    }

    /// <summary>
    /// Draws the source inside a box, as large as fits without distortion, centred.
    /// </summary>
    /// <param name="canvas">Where to draw.</param>
    /// <param name="source">The decoded upload.</param>
    /// <param name="box">The area to fit into.</param>
    private static void DrawFitted(SKCanvas canvas, SKBitmap source, SKRect box)
    {
        var scale = Math.Min(box.Width / source.Width, box.Height / source.Height);
        var width = source.Width * scale;
        var height = source.Height * scale;
        var left = box.Left + ((box.Width - width) / 2f);
        var top = box.Top + ((box.Height - height) / 2f);

        using var image = SKImage.FromBitmap(source);
        using var paint = new SKPaint { IsAntialias = true };
        canvas.DrawImage(image, new SKRect(left, top, left + width, top + height), _sampling, paint);
    }

    /// <summary>A blank RGBA bitmap in the format every rendition is drawn into.</summary>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <returns>A bitmap; the caller owns it.</returns>
    private static SKBitmap NewCanvasBitmap(int width, int height) =>
        new(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));

    /// <summary>Encodes an image as PNG.</summary>
    /// <param name="bitmap">The image to encode.</param>
    /// <returns>PNG bytes.</returns>
    private static byte[] Encode(SKBitmap bitmap)
    {
        using var pixmap = bitmap.PeekPixels();
        using var output = new MemoryStream();

        // Icons are flat colour and text, which is what adaptive filtering and a full zlib pass
        // are for. These files are written once and served for a year, so the encode cost is
        // paid once and the transfer saving is permanent.
        var encoded = pixmap.Encode(
            output,
            new SKPngEncoderOptions(SKPngEncoderFilterFlags.AllFilters, zLibLevel: 9));

        if (!encoded)
        {
            throw new InvalidOperationException("PNG encoding failed.");
        }

        return output.ToArray();
    }

    /// <summary>
    /// Whether the bytes look like SVG source.
    /// </summary>
    /// <remarks>
    /// A prefix scan rather than a parse. It exists only to turn what would otherwise be a
    /// generic "no decoder recognised this" into a message that tells the operator what to do,
    /// so a loose match costs nothing: anything it misses is refused a line later anyway.
    /// </remarks>
    /// <param name="content">The uploaded bytes.</param>
    /// <returns><see langword="true"/> when the leading text opens an SVG document.</returns>
    private static bool LooksLikeSvg(byte[] content)
    {
        // Enough to clear a UTF-8 BOM, an XML declaration, a doctype and leading whitespace.
        var prefixLength = Math.Min(content.Length, 1024);
        var prefix = System.Text.Encoding.UTF8.GetString(content, 0, prefixLength);

        return prefix.Contains("<svg", StringComparison.OrdinalIgnoreCase);
    }
}
