using System.Net;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Migration;
using Innovayse.Hostpanel.Domain.Migration.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Migration;

/// <summary>
/// <c>GET /api/admin/migrations/{id}/logs</c>, dispatching <c>GetMigrationLogsQuery</c>: one page
/// of a migration job's import report.
/// </summary>
/// <remarks>
/// Runs in CI, where Testcontainers can start PostgreSQL; on a laptop without a reachable docker
/// socket every test in this project fails before the API boots, and that is the environment, not
/// these tests.
/// </remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetMigrationLogsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A freshly created job's log page is empty.</summary>
    [Fact]
    public async Task LogsOnAFreshJobAreEmpty()
    {
        var client = await MigrationTestSeeds.AdminClientAsync(factory);
        var id = await MigrationTestSeeds.CreateJobAsync(client, "http://logs.example.test");

        var response = await client.GetAsync($"/api/admin/migrations/{id}/logs");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("totalCount").GetInt32().Should().Be(0);
        body.RootElement.GetProperty("items").EnumerateArray().Should().BeEmpty();
    }

    /// <summary>
    /// A request with no <c>page</c>/<c>pageSize</c> in the query string still returns the entry —
    /// the two now carry the same defaults the controller's own parameters used to, since a record
    /// bound from the query string has no method signature left to hang them on.
    /// </summary>
    [Fact]
    public async Task LogsWithNoPagingParametersUseTheDefaultsAndReturnTheEntry()
    {
        var client = await MigrationTestSeeds.AdminClientAsync(factory);
        var id = await MigrationTestSeeds.CreateJobAsync(client, "http://logs.example.test");
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
            context.MigrationLogs.Add(MigrationLog.Create(
                id, MigrationEntityType.Clients, "someone@example.test", MigrationLogAction.Imported));
            await context.SaveChangesAsync(CancellationToken.None);
        }

        var response = await client.GetAsync($"/api/admin/migrations/{id}/logs");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("totalCount").GetInt32().Should().Be(1);
        body.RootElement.GetProperty("page").GetInt32().Should().Be(1);
        body.RootElement.GetProperty("pageSize").GetInt32().Should().Be(50);
        body.RootElement.GetProperty("items").EnumerateArray().Should().ContainSingle();
    }

    /// <summary>A filter value that names no member of its enum is refused before the query runs.</summary>
    [Fact]
    public async Task LogsRefusesAnUnknownFilterValue()
    {
        var client = await MigrationTestSeeds.AdminClientAsync(factory);
        var id = await MigrationTestSeeds.CreateJobAsync(client, "http://logs.example.test");

        var response = await client.GetAsync($"/api/admin/migrations/{id}/logs?action=not-a-real-action");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task LogsRefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/admin/migrations/1/logs");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
