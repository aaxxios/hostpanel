using Innovayse.Hostpanel.Shared.Models.Interfaces;

namespace Innovayse.Hostpanel.Application.Common;

/// <summary>
/// A generated file handed back by a use case, ready for <c>BaseApiController.FileResult</c> to
/// stream as a download. Shared by every export and download query.
/// </summary>
/// <param name="FileContent">The file's raw bytes.</param>
/// <param name="ContentType">MIME type of the file, e.g. <c>text/csv</c>.</param>
/// <param name="FileName">Suggested download name, with extension.</param>
public sealed record FileDownloadDto(byte[] FileContent, string ContentType, string FileName) : IFileDownload;
