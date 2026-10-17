using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>POST /api/clients/{clientId}/billable-items/time-billing</c> against Postgres: batch
/// creating one billable item per timesheet entry.
/// </summary>
/// <remarks>Split from the deleted <c>BillableItemsEndpointTests</c> (A7 Task 3).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class CreateTimeBillingEntriesTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Creating time billing entries produces one billable item per entry.</summary>
    [Fact]
    public async Task CreateTimeBillingEntriesCreatesOneItemPerEntry()
    {
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var admin = await BillingTestSeeds.AdminClientAsync(factory);

        var response = await admin.PostAsJsonAsync($"/api/clients/{clientId}/billable-items/time-billing", new
        {
            entries = new[]
            {
                new { description = "Hour 1", hours = 2m, rate = 50m },
                new { description = "Hour 2", hours = 1m, rate = 50m },
            },
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var ids = await response.Content.ReadFromJsonAsync<int[]>();
        ids.Should().HaveCount(2);
    }

    /// <summary>Creating time billing entries is not for anonymous callers.</summary>
    [Fact]
    public async Task CreateTimeBillingEntriesRefusesAnonymous()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/clients/1/billable-items/time-billing", new
        {
            entries = new[] { new { description = "Hour 1", hours = 1m, rate = 50m } },
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
