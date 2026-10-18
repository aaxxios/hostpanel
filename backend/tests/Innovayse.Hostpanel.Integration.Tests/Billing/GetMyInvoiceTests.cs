using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>GET /api/me/invoices/{id}</c> against Postgres: a client reads one of their own invoices,
/// scoped through <c>IInvoiceOwnership</c>, which answers <c>INVOICE_NOT_FOUND</c> alike for a
/// stranger's invoice and for one that does not exist at all.
/// </summary>
/// <remarks>Split from the deleted <c>BillingEndpointTests</c> and <c>MyInvoicesEndpointTests</c> (A7 Task 3).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetMyInvoiceTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>The owner reads their own invoice by id.</summary>
    [Fact]
    public async Task GetById_TheOwnersInvoice_Returns200()
    {
        var (owner, ownerClientId) = await BillingTestSeeds.RegisterClientAsync(factory, null, "owner-get");
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, ownerClientId, 10m);

        var response = await owner.GetAsync($"/api/me/invoices/{invoiceId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("id").GetInt32().Should().Be(invoiceId);
    }

    /// <summary>Reading a stranger's invoice is refused with <c>INVOICE_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task GetById_AStrangersInvoice_Returns404WithInvoiceNotFound()
    {
        var (_, ownerClientId) = await BillingTestSeeds.RegisterClientAsync(factory, null, "owner-strangers-get");
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, ownerClientId, 10m);
        var stranger = await BillingTestSeeds.RegisterStrangerAsync(factory, null, "stranger-get");

        var response = await stranger.GetAsync($"/api/me/invoices/{invoiceId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await BillingTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.INVOICE_NOT_FOUND);
    }

    /// <summary>
    /// Another client's invoice is not merely refused — it is indistinguishable from one that
    /// does not exist. An attacker walking sequential invoice ids must not learn which ones are
    /// real from the status alone.
    /// </summary>
    [Fact]
    public async Task GetById_AnUnknownInvoice_AnswersIdenticallyToAStrangersInvoice()
    {
        var (_, ownerClientId) = await BillingTestSeeds.RegisterClientAsync(factory, null, "owner-strangers-cmp");
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, ownerClientId, 10m);
        var stranger = await BillingTestSeeds.RegisterStrangerAsync(factory, null, "stranger-cmp");

        var response = await stranger.GetAsync($"/api/me/invoices/{invoiceId}");
        var absent = await stranger.GetAsync($"/api/me/invoices/{BillingTestSeeds.NoSuchId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        absent.StatusCode.Should().Be(response.StatusCode);
        (await absent.Content.ReadAsStringAsync()).Should().Be(await response.Content.ReadAsStringAsync());
    }

    /// <summary>An id naming no invoice at all answers exactly like a stranger's invoice.</summary>
    [Fact]
    public async Task GetById_AnUnknownInvoice_Returns404WithInvoiceNotFound()
    {
        var (client, _) = await BillingTestSeeds.RegisterClientAsync(factory, null, "unknown-get");

        var response = await client.GetAsync($"/api/me/invoices/{BillingTestSeeds.NoSuchId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await BillingTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.INVOICE_NOT_FOUND);
    }

    /// <summary>Reading an invoice is not for anonymous callers.</summary>
    [Fact]
    public async Task GetByIdRefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/me/invoices/1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
