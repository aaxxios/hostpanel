using System.Net;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>GET /api/transactions</c> against Postgres: the admin-wide, paginated transaction ledger.
/// </summary>
/// <remarks>Split from the deleted <c>TransactionsEndpointTests</c> (A7 Task 3).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ListTransactionsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A transaction just created appears in the admin's list.</summary>
    [Fact]
    public async Task ListIncludesAFreshlyCreatedTransaction()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var transactionId = await BillingTestSeeds.CreateTransactionAsync(admin, clientId, 8m);

        var response = await admin.GetAsync("/api/transactions?page=1&pageSize=100");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("transactions").GetProperty("items").EnumerateArray()
            .Select(i => i.GetProperty("id").GetInt32()).Should().Contain(transactionId);
    }

    /// <summary>The transaction endpoints are not for anonymous callers.</summary>
    [Fact]
    public async Task ListRefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/transactions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
