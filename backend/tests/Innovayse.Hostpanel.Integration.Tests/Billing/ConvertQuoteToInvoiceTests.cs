using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>POST /api/billing/quotes/{id}/convert-to-invoice</c> against Postgres: converting a quote to
/// a draft invoice, the refusal to convert an already-accepted quote a second time, and the
/// <c>QUOTE_NOT_FOUND</c> refusal for an id that names no quote.
/// </summary>
/// <remarks>Split from the deleted <c>QuotesEndpointTests</c> (A7 Task 3).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ConvertQuoteToInvoiceTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// Converting an accepted quote a second time answers the aggregate's own stage invariant,
    /// which still throws rather than becoming a refusal code (a business rule, not a lookup), and
    /// the exception middleware turns that into a 400.
    /// </summary>
    [Fact]
    public async Task ConvertingAnAlreadyAcceptedQuoteRefusesBadRequest()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var quoteId = await BillingTestSeeds.CreateQuoteAsync(admin, clientId, 10m);

        var firstConvert = await admin.PostAsync($"/api/billing/quotes/{quoteId}/convert-to-invoice", null);
        firstConvert.StatusCode.Should().Be(HttpStatusCode.Created);

        var secondConvert = await admin.PostAsync($"/api/billing/quotes/{quoteId}/convert-to-invoice", null);
        secondConvert.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>Converting an id that names no quote answers 404 with <c>QUOTE_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task ConvertUnknownQuoteRefusesNotFound()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);

        var response = await admin.PostAsync($"/api/billing/quotes/{BillingTestSeeds.NoSuchId}/convert-to-invoice", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await BillingTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.QUOTE_NOT_FOUND);
    }

    /// <summary>Converting a quote is not for anonymous callers.</summary>
    [Fact]
    public async Task ConvertRefusesAnonymous()
    {
        var response = await factory.CreateClient().PostAsync("/api/billing/quotes/1/convert-to-invoice", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
