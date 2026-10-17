using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Migration;

/// <summary>
/// <c>DELETE /api/admin/migrations/{id}</c>, dispatching <c>DeleteMigrationJobCommand</c>: deletes
/// a migration job the operator no longer wants to keep a record of.
/// </summary>
/// <remarks>
/// Runs in CI, where Testcontainers can start PostgreSQL; on a laptop without a reachable docker
/// socket every test in this project fails before the API boots, and that is the environment, not
/// these tests.
/// </remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class DeleteMigrationJobTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Deleting an existing job removes it; getting it afterwards answers 404.</summary>
    [Fact]
    public async Task DeleteRemovesTheJob()
    {
        var client = await MigrationTestSeeds.AdminClientAsync(factory);
        var id = await MigrationTestSeeds.CreateJobAsync(client, "http://delete-me.example.test");

        var deleted = await client.DeleteAsync($"/api/admin/migrations/{id}");
        deleted.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterwards = await client.GetAsync($"/api/admin/migrations/{id}");
        afterwards.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await MigrationTestSeeds.AssertCodeAsync(afterwards, HostpanelErrorCodes.MIGRATION_JOB_NOT_FOUND);
    }

    /// <summary>An id naming no job is refused as missing.</summary>
    [Fact]
    public async Task DeleteRefusesAnUnknownId()
    {
        var client = await MigrationTestSeeds.AdminClientAsync(factory);
        const int missing = 987654321;

        var response = await client.DeleteAsync($"/api/admin/migrations/{missing}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await MigrationTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.MIGRATION_JOB_NOT_FOUND);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task DeleteRefusesAnonymous()
    {
        var response = await factory.CreateClient().DeleteAsync("/api/admin/migrations/1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
