using System.Diagnostics.CodeAnalysis;

namespace Innovayse.Hostpanel.Shared.Models.Interfaces;

/// <summary>
///     A handler result that the API streams back as a file download. Implemented by the export
///     DTOs so <c>BaseApiController.FileResult</c> can turn a <see cref="Result{T}"/> into a
///     <c>FileContentResult</c> without the controller touching the bytes itself.
/// </summary>
[SuppressMessage(
    "Performance",
    "CA1819:Properties should not return arrays",
    Justification = "House contract: the download bytes are handed to FileContentResult as-is.")]
public interface IFileDownload
{
    /// <summary>Raw file bytes.</summary>
    public byte[] FileContent { get; }

    /// <summary>Suggested download name (with extension).</summary>
    public string FileName { get; }

    /// <summary>MIME type of <see cref="FileContent"/>.</summary>
    public string ContentType { get; }
}
