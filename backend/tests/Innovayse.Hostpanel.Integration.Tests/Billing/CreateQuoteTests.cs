using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>POST /api/billing/quotes</c> against Postgres: creating a quote for a client, then reading
/// it back round-trips the line item and total.
/// </summary>
/// <remarks>Split from the deleted <c>QuotesEndpointTests</c> (A7 Task 3).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class CreateQuoteTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Creating a quote, then reading it back, round-trips the line item and total.</summary>
    [Fact]
    public async Task CreateThenGetRoundTripsTheQuote()
    {
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var admin = await BillingTestSeeds.AdminClientAsync(factory);

        var quoteId = await BillingTestSeeds.CreateQuoteAsync(admin, clientId, 500m);

        var fetched = await admin.GetAsync($"/api/billing/quotes/{quoteId}");
        fetched.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await fetched.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("id").GetInt32().Should().Be(quoteId);
        body.RootElement.GetProperty("total").GetDecimal().Should().Be(500m);
    }

    /// <summary>Creating a quote is not for anonymous callers.</summary>
    [Fact]
    public async Task CreateRefusesAnonymous()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/billing/quotes", new
        {
            clientId = 1,
            subject = "Website redesign",
            expiryDate = DateTimeOffset.UtcNow.AddDays(30),
            notes = (string?)null,
            items = new[] { new { description = "Design", unitPrice = 500m, quantity = 1 } },
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
