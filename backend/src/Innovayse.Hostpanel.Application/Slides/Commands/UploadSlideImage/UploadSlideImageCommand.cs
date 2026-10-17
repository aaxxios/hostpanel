using Innovayse.Hostpanel.Shared.Contracts.Interfaces;

namespace Innovayse.Hostpanel.Application.Slides.Commands.UploadSlideImage;

/// <summary>Command that stores a new slide image and reports the URL it is served from.</summary>
/// <remarks>
/// Carries the stream rather than the <c>IFormFile</c> it arrived in: <c>IFormFile</c> is an
/// ASP.NET Core type, and a command that names it would drag the web framework into the
/// Application layer for no gain. The handler does the reading.
/// </remarks>
/// <param name="Content">The uploaded stream; the caller owns and disposes it.</param>
/// <param name="Length">The declared length of the upload, in bytes.</param>
/// <param name="ContentType">The media type the upload declared, e.g. <c>image/png</c>.</param>
/// <param name="FileName">
/// The untrusted name the browser sent, read only for its extension — never used as the stored
/// file name.
/// </param>
public sealed record UploadSlideImageCommand(Stream Content, long Length, string ContentType, string FileName) : IDemoRestricted;
