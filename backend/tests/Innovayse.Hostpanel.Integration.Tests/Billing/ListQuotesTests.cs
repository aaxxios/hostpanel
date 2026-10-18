using System.Net;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>GET /api/billing/quotes</c> against Postgres: the admin-wide, paginated quote list.
/// </summary>
/// <remarks>New coverage — gap-filled for A7 Task 3, ruling 2; no grouped file exercised this route.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ListQuotesTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A quote just created appears in the admin's list.</summary>
    [Fact]
    public async Task ListIncludesAFreshlyCreatedQuote()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var quoteId = await BillingTestSeeds.CreateQuoteAsync(admin, clientId, 90m);

        var response = await admin.GetAsync("/api/billing/quotes?page=1&pageSize=100");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("items").EnumerateArray()
            .Select(i => i.GetProperty("id").GetInt32()).Should().Contain(quoteId);
    }

    /// <summary>Listing quotes is not for anonymous callers.</summary>
    [Fact]
    public async Task ListRefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/billing/quotes");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
