using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>POST /api/billing/{id}/credit</c> against Postgres: applying credit reduces the invoice's
/// total owed, and the <c>INVOICE_NOT_FOUND</c> refusal for an id that names no invoice.
/// </summary>
/// <remarks>Split from the deleted <c>InvoicePaymentsEndpointTests</c> (A7 Task 3).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ApplyInvoiceCreditTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Applying credit reduces the invoice's total owed.</summary>
    [Fact]
    public async Task ApplyCreditReducesTheTotal()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 19.99m);

        var response = await admin.PostAsJsonAsync($"/api/billing/{invoiceId}/credit", new { amount = 5m });
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var fetched = await admin.GetAsync($"/api/billing/{invoiceId}");
        using var body = JsonDocument.Parse(await fetched.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("total").GetDecimal().Should().Be(14.99m);
    }

    /// <summary>Applying credit against an id that names no invoice answers 404 with <c>INVOICE_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task ApplyCreditRefusesAnUnknownInvoice()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);

        var response = await admin.PostAsJsonAsync($"/api/billing/{BillingTestSeeds.NoSuchId}/credit", new { amount = 5m });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await BillingTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.INVOICE_NOT_FOUND);
    }

    /// <summary>Applying credit is not for anonymous callers.</summary>
    [Fact]
    public async Task ApplyCreditRefusesAnonymous()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/billing/1/credit", new { amount = 5m });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
