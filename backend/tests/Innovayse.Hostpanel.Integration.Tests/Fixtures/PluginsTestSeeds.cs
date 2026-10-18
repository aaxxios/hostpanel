using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Innovayse.Hostpanel.Integration.Tests.Fixtures;

/// <summary>
/// Shared seeding and authentication helpers for the Admin/Plugins module's per-use-case
/// Integration tests: building well-formed and malformed plugin ZIP archives, an authenticated
/// admin or Client-role caller, and reading a refusal's error code. Every
/// <c>Admin/Plugins/*Tests.cs</c> file calls into this rather than repeating its own copy. Moved
/// out of the deleted <c>PluginsEndpointTests</c>.
/// </summary>
internal static class PluginsTestSeeds
{
    /// <summary>Shared test password for every client user this class registers.</summary>
    public const string TestPassword = "Pass@123!";

    /// <summary>A client carrying the seeded admin's bearer token.</summary>
    /// <param name="factory">The shared API + PostgreSQL host.</param>
    /// <returns>An authenticated client with the Admin role.</returns>
    public static async Task<HttpClient> AdminClientAsync(IntegrationTestFactory factory)
    {
        var client = factory.CreateClient();
        var token = await factory.GetAdminTokenAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>
    /// Registers a brand-new client user and returns an authenticated <see cref="HttpClient"/>
    /// carrying only the Client role -- the caller every admin-only boundary test refuses.
    /// </summary>
    /// <param name="factory">The shared API + PostgreSQL host.</param>
    /// <param name="label">A short label folded into the unique email, for readable test failures.</param>
    /// <returns>An authenticated client with no elevated role.</returns>
    public static async Task<HttpClient> ClientRoleClientAsync(IntegrationTestFactory factory, string label)
    {
        var httpClient = factory.CreateClient();
        var email = $"{label}-{Guid.NewGuid():N}@example.com";
        var registerResponse = await httpClient.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = TestPassword,
            firstName = "Plugin",
            lastName = "Boundary",
        });
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var token = await factory.GetClientTokenAsync(email, TestPassword);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return httpClient;
    }

    /// <summary>Asserts the refusal carries the expected error code.</summary>
    /// <param name="response">The refusal.</param>
    /// <param name="expected">The code it must carry.</param>
    /// <returns>A task that completes once the body has been read.</returns>
    public static async Task AssertCodeAsync(HttpResponseMessage response, string expected)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("type").GetString().Should().Be(expected);
    }

    /// <summary>Builds a well-formed plugin ZIP with the given id and one harmless file.</summary>
    /// <param name="pluginId">The plugin id to write into <c>plugin.json</c>.</param>
    /// <returns>The ZIP archive's raw bytes.</returns>
    public static byte[] BuildValidPluginZip(string pluginId)
    {
        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            var manifest = archive.CreateEntry("plugin.json");
            using (var writer = new StreamWriter(manifest.Open()))
            {
                writer.Write($$"""
                    {
                      "id": "{{pluginId}}",
                      "name": "Test Plugin",
                      "version": "1.0.0",
                      "author": "Test",
                      "description": "A plugin used only in tests.",
                      "type": "Provisioning",
                      "category": "Test",
                      "entryPoint": "Test.Plugin",
                      "sdkVersion": "1.0"
                    }
                    """);
            }

            var readme = archive.CreateEntry("readme.txt");
            using (var writer = new StreamWriter(readme.Open()))
            {
                writer.Write("harmless file");
            }
        }

        return ms.ToArray();
    }

    /// <summary>Builds a ZIP with no <c>plugin.json</c> at its root.</summary>
    /// <returns>The ZIP archive's raw bytes.</returns>
    public static byte[] BuildZipWithoutManifest()
    {
        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = archive.CreateEntry("readme.txt");
            using var writer = new StreamWriter(entry.Open());
            writer.Write("no manifest here");
        }

        return ms.ToArray();
    }

    /// <summary>Posts a multipart install request carrying the given ZIP bytes.</summary>
    /// <param name="client">Authenticated admin client.</param>
    /// <param name="zipBytes">The archive to upload.</param>
    /// <returns>The install response.</returns>
    public static async Task<HttpResponseMessage> InstallAsync(HttpClient client, byte[] zipBytes)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(zipBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        content.Add(fileContent, "file", "plugin.zip");
        return await client.PostAsync("/api/admin/plugins/install", content);
    }
}
