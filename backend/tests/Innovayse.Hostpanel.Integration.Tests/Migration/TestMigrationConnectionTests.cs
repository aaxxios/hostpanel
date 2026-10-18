using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Migration.Common;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Migration;

/// <summary>
/// <c>POST /api/admin/migrations/{id}/test-connection</c>, dispatching
/// <c>TestMigrationConnectionQuery</c>: tests the connection from this panel to the legacy
/// system's migration plugin.
/// </summary>
/// <remarks>
/// Runs in CI, where Testcontainers can start PostgreSQL; on a laptop without a reachable docker
/// socket every test in this project fails before the API boots, and that is the environment, not
/// these tests.
/// </remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class TestMigrationConnectionTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A fake source that always answers a ping, standing in for a reachable plugin.</summary>
    private sealed class AlwaysReachableSource : IMigrationSource
    {
        /// <inheritdoc/>
        public Task PingAsync(string sourceUrl, string accessKey, CancellationToken ct = default) =>
            Task.CompletedTask;

        /// <inheritdoc/>
        public Task<MigrationSourceTotals> GetTotalsAsync(string sourceUrl, string accessKey, CancellationToken ct = default) =>
            Task.FromResult(new MigrationSourceTotals(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0));

        /// <inheritdoc/>
        public Task<MigrationRecordPage<TRecord>> GetRecordPageAsync<TRecord>(
            string sourceUrl, string accessKey, string dataSet, int page, int perPage, CancellationToken ct = default) =>
            Task.FromResult(new MigrationRecordPage<TRecord>([], 0));
    }

    /// <summary>
    /// A connection test against a reachable source succeeds and records the ping, run on an
    /// isolated host sharing the same seeded Postgres so the admin token stays valid there.
    /// </summary>
    [Fact]
    public async Task TestConnectionAgainstAReachableSourceSucceeds()
    {
        var token = await factory.GetAdminTokenAsync();
        using var host = MigrationTestSeeds.HostWithFakeSource(factory, new AlwaysReachableSource());
        var client = host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var id = await MigrationTestSeeds.CreateJobAsync(client, "http://reachable.example.test");

        var response = await client.PostAsync($"/api/admin/migrations/{id}/test-connection", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>Testing the connection against a source nothing answers is a 502, with no source detail.</summary>
    [Fact]
    public async Task TestConnectionAgainstAnUnreachableSourceAnswersUpstreamFailed()
    {
        var client = await MigrationTestSeeds.AdminClientAsync(factory);
        var id = await MigrationTestSeeds.CreateJobAsync(client, MigrationTestSeeds.UnreachableSourceUrl);

        var response = await client.PostAsync($"/api/admin/migrations/{id}/test-connection", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
        await MigrationTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.MIGRATION_CONNECTION_UPSTREAM_FAILED);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContain("127.0.0.1");
    }

    /// <summary>An id naming no job is refused as missing.</summary>
    [Fact]
    public async Task TestConnectionRefusesAnUnknownId()
    {
        var client = await MigrationTestSeeds.AdminClientAsync(factory);
        const int missing = 987654321;

        var response = await client.PostAsync($"/api/admin/migrations/{missing}/test-connection", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await MigrationTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.MIGRATION_JOB_NOT_FOUND);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task TestConnectionRefusesAnonymous()
    {
        var response = await factory.CreateClient().PostAsync("/api/admin/migrations/1/test-connection", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
