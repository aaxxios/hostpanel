using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// A host environment rooted in a directory the test owns, for adapters that resolve files under the
/// web root without starting the whole application.
/// </summary>
public sealed class StubWebHostEnvironment : IWebHostEnvironment
{
    /// <summary>Initializes the environment with both roots set to <paramref name="root"/>.</summary>
    /// <param name="root">The directory that stands in for the content root and the web root.</param>
    public StubWebHostEnvironment(string root)
    {
        ContentRootPath = root;
        WebRootPath = root;
        ContentRootFileProvider = new NullFileProvider();
        WebRootFileProvider = new NullFileProvider();
    }

    /// <inheritdoc/>
    public string WebRootPath { get; set; }

    /// <inheritdoc/>
    public IFileProvider WebRootFileProvider { get; set; }

    /// <inheritdoc/>
    public string ApplicationName { get; set; } = "Innovayse.Hostpanel.Integration.Tests";

    /// <inheritdoc/>
    public IFileProvider ContentRootFileProvider { get; set; }

    /// <inheritdoc/>
    public string ContentRootPath { get; set; }

    /// <inheritdoc/>
    public string EnvironmentName { get; set; } = "Testing";
}
