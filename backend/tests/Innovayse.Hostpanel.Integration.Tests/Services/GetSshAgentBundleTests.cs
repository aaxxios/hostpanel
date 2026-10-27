using System.Formats.Tar;
using System.IO.Compression;
using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;

namespace Innovayse.Hostpanel.Integration.Tests.Services;

/// <summary><c>GET api/public/ssh-agent/bundle.tar.gz</c>: anonymous, stable, and only what an installer needs.</summary>
public sealed class GetSshAgentBundleTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    [Fact]
    public async Task Bundle_IsAnonymousStableAndHoldsInstallerProfilesAndNoTestsAsync()
    {
        var http = factory.CreateClient();

        var response = await http.GetAsync("/api/public/ssh-agent/bundle.tar.gz");
        var again = await http.GetByteArrayAsync("/api/public/ssh-agent/bundle.tar.gz");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/gzip");
        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().Equal(again);

        var entries = new List<(string Name, UnixFileMode Mode)>();
        using (var tar = new TarReader(new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress)))
        {
            while (tar.GetNextEntry() is { } entry)
            {
                entries.Add((entry.Name, entry.Mode));
            }
        }

        var names = entries.Select(e => e.Name.Split('/', 2)[1]).ToList();
        names.Should().Contain(["hostpanel-ssh-key-agent", "install.sh", "uninstall.sh", "profiles/cwp.conf", "profiles/generic-linux.conf", "providers/cwp/account-check", "README.md"]);
        names.Should().NotContain(n => n.StartsWith("tests/") || n.Contains("__pycache__"));
        entries.Single(e => e.Name.EndsWith("/install.sh")).Mode.Should().HaveFlag(UnixFileMode.UserExecute);
        entries.Single(e => e.Name.EndsWith("/README.md")).Mode.Should().NotHaveFlag(UnixFileMode.UserExecute);
    }
}
