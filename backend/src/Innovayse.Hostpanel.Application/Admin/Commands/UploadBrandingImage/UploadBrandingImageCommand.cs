using Innovayse.Hostpanel.Shared.Contracts.Interfaces;

using Innovayse.Hostpanel.Application.Admin.Common;
using Innovayse.Hostpanel.Application.Admin.Common.Enums;

namespace Innovayse.Hostpanel.Application.Admin.Commands.UploadBrandingImage;

/// <summary>
/// Command that stores a new storefront branding image and everything derived from it.
/// </summary>
/// <remarks>
/// Carries the stream rather than the <c>IFormFile</c> it arrived in: <c>IFormFile</c> is an
/// ASP.NET Core type, and a command that names it would drag the web framework into the
/// Application layer for no gain. The request-size limit on the action caps the body; the
/// handler reads the stream and hands the bytes to the image processor.
/// </remarks>
/// <param name="Kind">
/// Which branding image this is, as the route sent it — the <see cref="BrandingKind"/> member
/// name, case-insensitive (<c>logo</c>, <c>logodark</c>, <c>logomark</c>, <c>favicon</c>). Carried
/// as the raw string rather than the parsed enum: the validator is the one place that decides
/// whether it names a real kind, so a caller-steered value never reaches the handler unchecked.
/// </param>
/// <param name="Content">The uploaded stream; the caller owns and disposes it.</param>
/// <param name="Length">The declared length of the upload, in bytes.</param>
/// <param name="FileName">The untrusted client-supplied name, for diagnostics only.</param>
public sealed record UploadBrandingImageCommand(string Kind, Stream Content, long Length, string FileName) : IDemoRestricted;
