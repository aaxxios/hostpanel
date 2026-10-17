using System.Net;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>GET /api/billing/{id}</c> against Postgres: reading a single invoice with its line items,
/// and the <c>INVOICE_NOT_FOUND</c> refusal for an id that names no invoice.
/// </summary>
/// <remarks>Split from the deleted <c>InvoicesEndpointTests</c> (A7 Task 3).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetInvoiceTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An admin reads back the invoice they just created.</summary>
    [Fact]
    public async Task GetByIdReturnsTheCreatedInvoice()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 12.50m);

        var response = await admin.GetAsync($"/api/billing/{invoiceId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("id").GetInt32().Should().Be(invoiceId);
        body.RootElement.GetProperty("clientId").GetInt32().Should().Be(clientId);
    }

    /// <summary>Reading an id that names no invoice answers 404 with the <c>INVOICE_NOT_FOUND</c> code.</summary>
    [Fact]
    public async Task GetUnknownInvoiceRefusesNotFound()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);

        var response = await admin.GetAsync($"/api/billing/{BillingTestSeeds.NoSuchId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await BillingTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.INVOICE_NOT_FOUND);
    }

    /// <summary>Reading an invoice is not for anonymous callers.</summary>
    [Fact]
    public async Task GetRefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/billing/1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
