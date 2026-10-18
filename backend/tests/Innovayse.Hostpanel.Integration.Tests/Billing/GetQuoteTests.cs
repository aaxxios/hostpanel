using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>GET /api/billing/quotes/{id}</c> against Postgres: reading a single quote, and the
/// <c>QUOTE_NOT_FOUND</c> refusal for an id that names no quote.
/// </summary>
/// <remarks>Split from the deleted <c>QuotesEndpointTests</c> (A7 Task 3).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetQuoteTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An admin reads back the quote they just created.</summary>
    [Fact]
    public async Task GetByIdReturnsTheCreatedQuote()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var quoteId = await BillingTestSeeds.CreateQuoteAsync(admin, clientId, 250m);

        var response = await admin.GetAsync($"/api/billing/quotes/{quoteId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>Reading an id that names no quote answers 404 with the <c>QUOTE_NOT_FOUND</c> code.</summary>
    [Fact]
    public async Task GetUnknownQuoteRefusesNotFound()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);

        var response = await admin.GetAsync($"/api/billing/quotes/{BillingTestSeeds.NoSuchId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await BillingTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.QUOTE_NOT_FOUND);
    }

    /// <summary>Reading a quote is not for anonymous callers.</summary>
    [Fact]
    public async Task GetRefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/billing/quotes/1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
