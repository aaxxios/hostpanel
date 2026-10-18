namespace Innovayse.Hostpanel.Application.Common.Interfaces;

/// <summary>Writes an uploaded slide image and reports the URL it is served from.</summary>
public interface ISlideImageStorage
{
    /// <summary>
    /// Saves the bytes under a generated name and returns the URL the storefront can load them
    /// from.
    /// </summary>
    /// <param name="content">The image bytes.</param>
    /// <param name="extension">
    /// The file extension to save under, dot included (e.g. <c>.png</c>); never taken from the
    /// caller's own file name, so nothing an operator typed reaches a path.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The root-relative URL the saved file is served from.</returns>
    Task<string> SaveAsync(byte[] content, string extension, CancellationToken ct);
}
