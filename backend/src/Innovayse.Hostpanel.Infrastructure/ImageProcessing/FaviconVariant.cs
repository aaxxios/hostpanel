using Innovayse.Hostpanel.Application.Admin.Common.Enums;

namespace Innovayse.Hostpanel.Infrastructure.ImageProcessing;

/// <summary>One rendition of the favicon set: its file name, pixel size and role.</summary>
/// <param name="File">The file name.</param>
/// <param name="Size">The square size in pixels.</param>
/// <param name="Role">What the rendition is used for.</param>
internal sealed record FaviconVariant(string File, int Size, BrandingRole Role);
