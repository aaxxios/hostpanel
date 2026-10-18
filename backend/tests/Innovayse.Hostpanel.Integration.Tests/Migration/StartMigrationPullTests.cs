using System.Net;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Migration;

/// <summary>
/// <c>POST /api/admin/migrations/{id}/start</c>, dispatching <c>StartMigrationPullCommand</c>:
/// starts pulling data from the legacy system into this panel, in the background.
/// </summary>
/// <remarks>
/// Runs in CI, where Testcontainers can start PostgreSQL; on a laptop without a reachable docker
/// socket every test in this project fails before the API boots, and that is the environment, not
/// these tests.
/// </remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class StartMigrationPullTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Starting a pending job launches it and answers with the job's (still pending) state.</summary>
    [Fact]
    public async Task StartOnAPendingJobLaunchesThePull()
    {
        var client = await MigrationTestSeeds.AdminClientAsync(factory);
        var id = await MigrationTestSeeds.CreateJobAsync(client, MigrationTestSeeds.UnreachableSourceUrl);

        var response = await client.PostAsync($"/api/admin/migrations/{id}/start", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("id").GetInt32().Should().Be(id);
    }

    /// <summary>A job that is not Pending refuses to be started again.</summary>
    [Fact]
    public async Task StartRefusesAJobThatIsNotPending()
    {
        var client = await MigrationTestSeeds.AdminClientAsync(factory);
        var id = await MigrationTestSeeds.CreateJobAsync(client, MigrationTestSeeds.UnreachableSourceUrl);

        // Flipped directly through the real data context rather than by racing the background
        // worker this endpoint itself launches — the point under test is the status guard, not
        // how quickly a real pull reaches InProgress.
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
            var job = await context.MigrationJobs.FirstAsync(j => j.Id == id);
            job.Start(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            await context.SaveChangesAsync(CancellationToken.None);
        }

        var response = await client.PostAsync($"/api/admin/migrations/{id}/start", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await MigrationTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.MIGRATION_JOB_NOT_PENDING);
    }

    /// <summary>An id naming no job is refused as missing.</summary>
    [Fact]
    public async Task StartRefusesAnUnknownId()
    {
        var client = await MigrationTestSeeds.AdminClientAsync(factory);
        const int missing = 987654321;

        var response = await client.PostAsync($"/api/admin/migrations/{missing}/start", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await MigrationTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.MIGRATION_JOB_NOT_FOUND);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task StartRefusesAnonymous()
    {
        var response = await factory.CreateClient().PostAsync("/api/admin/migrations/1/start", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
