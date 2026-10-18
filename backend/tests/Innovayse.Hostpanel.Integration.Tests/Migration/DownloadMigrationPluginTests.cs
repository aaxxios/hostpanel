using System.Net;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Migration;

/// <summary>
/// <c>GET /api/migrations/plugin/download</c>, dispatching <c>DownloadMigrationPluginQuery</c>.
/// Public and anonymous: the legacy system being migrated away from has no credential this
/// platform would recognise when it fetches its importer plugin.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DownloadMigrationPluginTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// No credential is required, and the answer is one of exactly two well-formed shapes: the
    /// zip archive when this checkout carries the plugin's files, or the coded 404 refusal when it
    /// does not -- whichever this build's <c>plugins/innovayse_migration</c> folder decides, the
    /// response is never an unmapped fault.
    /// </summary>
    [Fact]
    public async Task DownloadPlugin_IsAnonymousAndAnswersAZipOrTheCodedRefusalAsync()
    {
        var response = await factory.CreateClient().GetAsync("/api/migrations/plugin/download");

        if (response.StatusCode == HttpStatusCode.OK)
        {
            response.Content.Headers.ContentType!.MediaType.Should().Be("application/zip");
            return;
        }

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("type").GetString().Should().Be(HostpanelErrorCodes.MIGRATION_PLUGIN_NOT_FOUND);
        body.RootElement.GetProperty("detail").GetString().Should().NotBeNullOrWhiteSpace();
    }
}
