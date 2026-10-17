using Innovayse.Hostpanel.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace Innovayse.Hostpanel.Infrastructure.Storage;

/// <summary>Writes a slide image under the API's own <c>wwwroot/uploads/slides</c> directory.</summary>
public sealed class LocalSlideImageStorage : ISlideImageStorage
{
    /// <summary>Web host environment, for resolving the upload path.</summary>
    private readonly IWebHostEnvironment _env;

    /// <summary>
    ///     Initializes the adapter.
    /// </summary>
    /// <param name="env">Web host environment, for resolving the upload path.</param>
    public LocalSlideImageStorage(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <inheritdoc/>
    public async Task<string> SaveAsync(byte[] content, string extension, CancellationToken ct)
    {
        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var uploadsDir = Path.Combine(webRoot, "uploads", "slides");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await File.WriteAllBytesAsync(filePath, content, ct);

        return $"/uploads/slides/{fileName}";
    }
}
