using System.Net;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>POST /api/me/invoices/{id}/pay</c> against Postgres: a client pays their own invoice
/// through the configured "charge now" gateway, scoped through <c>IInvoiceOwnership</c>.
/// </summary>
/// <remarks>Split from the deleted <c>MyInvoicesEndpointTests</c> (A7 Task 3).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class PayMyInvoiceTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>The owner pays their own invoice, which is marked paid.</summary>
    [Fact]
    public async Task Pay_TheOwnersInvoice_MarksItPaid()
    {
        var (owner, ownerClientId) = await BillingTestSeeds.RegisterClientAsync(factory, null, "owner-pay");
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, ownerClientId, 10m);

        var response = await owner.PostAsync($"/api/me/invoices/{invoiceId}/pay", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var fetched = await admin.GetAsync($"/api/billing/{invoiceId}");
        using var body = JsonDocument.Parse(await fetched.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("status").GetString().Should().Be(nameof(InvoiceStatus.Paid));
    }

    /// <summary>
    /// Paying a stranger's invoice is refused with <c>INVOICE_NOT_FOUND</c> before the shared
    /// write ever runs, so the invoice stays unpaid.
    /// </summary>
    [Fact]
    public async Task Pay_AStrangersInvoice_Returns404WithInvoiceNotFoundAndLeavesItUnpaid()
    {
        var (_, ownerClientId) = await BillingTestSeeds.RegisterClientAsync(factory, null, "owner-strangers-pay");
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, ownerClientId, 10m);
        var stranger = await BillingTestSeeds.RegisterStrangerAsync(factory, null, "stranger-pay");

        var response = await stranger.PostAsync($"/api/me/invoices/{invoiceId}/pay", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await BillingTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.INVOICE_NOT_FOUND);

        var fetched = await admin.GetAsync($"/api/billing/{invoiceId}");
        using var fetchedBody = JsonDocument.Parse(await fetched.Content.ReadAsStringAsync());
        fetchedBody.RootElement.GetProperty("status").GetString().Should().Be(nameof(InvoiceStatus.Unpaid));
    }

    /// <summary>Paying an id that names no invoice answers 404 with <c>INVOICE_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task Pay_AnUnknownInvoice_Returns404WithInvoiceNotFound()
    {
        var (owner, _) = await BillingTestSeeds.RegisterClientAsync(factory, null, "owner-unknown-pay");

        var response = await owner.PostAsync($"/api/me/invoices/{BillingTestSeeds.NoSuchId}/pay", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await BillingTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.INVOICE_NOT_FOUND);
    }

    /// <summary>Paying an invoice is not for anonymous callers.</summary>
    [Fact]
    public async Task PayRefusesAnonymous()
    {
        var response = await factory.CreateClient().PostAsync("/api/me/invoices/1/pay", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
