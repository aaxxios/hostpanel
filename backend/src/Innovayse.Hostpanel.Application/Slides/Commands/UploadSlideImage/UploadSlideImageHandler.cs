using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Shared.Models;

namespace Innovayse.Hostpanel.Application.Slides.Commands.UploadSlideImage;

/// <summary>Validates an uploaded slide image against the ceilings this deployment enforces, then stores it.</summary>
public sealed class UploadSlideImageHandler
{
    /// <summary>Longest an uploaded slide image may be, in bytes.</summary>
    private const long MaxContentLength = 5 * 1024 * 1024;

    /// <summary>Content types accepted for a slide image.</summary>
    private static readonly string[] AllowedContentTypes =
        ["image/jpeg", "image/png", "image/webp", "image/gif", "image/svg+xml"];

    /// <summary>Writes the accepted bytes and reports the URL they are served from.</summary>
    private readonly ISlideImageStorage _storage;

    /// <summary>
    ///     Initializes the handler.
    /// </summary>
    /// <param name="storage">Writes the accepted bytes and reports the URL they are served from.</param>
    public UploadSlideImageHandler(ISlideImageStorage storage)
    {
        _storage = storage;
    }

    /// <summary>
    ///     Handles the command.
    /// </summary>
    /// <param name="cmd">The uploaded bytes, media type and untrusted file name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The stored file's URL, or <c>SLIDE_IMAGE_TYPE_INVALID</c> when the media type is not one
    /// this deployment accepts for a slide image, or <c>SLIDE_IMAGE_EXCEEDS_LIMIT</c> when the
    /// upload is over the byte ceiling.
    /// </returns>
    public async Task<Result<SlideImageDto>> HandleAsync(UploadSlideImageCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        if (!AllowedContentTypes.Contains(cmd.ContentType))
        {
            return Result<SlideImageDto>.FailureCode(HostpanelErrorCodes.SLIDE_IMAGE_TYPE_INVALID);
        }

        if (cmd.Length > MaxContentLength)
        {
            return Result<SlideImageDto>.FailureCode(HostpanelErrorCodes.SLIDE_IMAGE_EXCEEDS_LIMIT);
        }

        var extension = Path.GetExtension(cmd.FileName).ToLowerInvariant();
        using var buffer = new MemoryStream();
        await cmd.Content.CopyToAsync(buffer, ct);
        var url = await _storage.SaveAsync(buffer.ToArray(), extension, ct);
        return Result<SlideImageDto>.Success(new SlideImageDto(url));
    }
}
