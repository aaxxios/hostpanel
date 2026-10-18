using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// The admin "charge now" endpoint (<c>POST /api/billing/{id}/pay</c>) against Postgres, with the
/// direct-charge gateway replaced by a fake: an invoice is charged in the currency it was issued
/// in, never a caller-chosen or default one.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class PayInvoiceTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// Builds an admin client whose host charges through <paramref name="gateway"/> instead of a
    /// real payment provider.
    /// </summary>
    /// <param name="gateway">The fake gateway the host should charge through.</param>
    /// <returns>An authenticated client.</returns>
    private async Task<HttpClient> AdminClientAsync(FakePaymentGateway gateway)
    {
        var host = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.AddScoped<Innovayse.Hostpanel.Application.Common.Interfaces.IPaymentGateway>(_ => gateway)));

        var token = await factory.GetAdminTokenAsync();
        var client = host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>
    /// An invoice is charged in the currency it was issued in. The command used to carry a
    /// caller-chosen code defaulting to USD, which charged an AMD invoice's total as dollars.
    /// </summary>
    [Fact]
    public async Task PayChargesInTheInvoicesOwnCurrency()
    {
        var gateway = new FakePaymentGateway();
        var admin = await AdminClientAsync(gateway);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory, currency: "AMD");
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 1200m);

        var response = await admin.PostAsync($"/api/billing/{invoiceId}/pay", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        gateway.LastRequest.Should().NotBeNull();
        gateway.LastRequest!.Currency.Should().Be("AMD");
        gateway.LastRequest.Amount.Should().Be(1200m);

        var fetched = await admin.GetAsync($"/api/billing/{invoiceId}");
        using var body = JsonDocument.Parse(await fetched.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("status").GetString().Should().Be(nameof(InvoiceStatus.Paid));
    }

    /// <summary>
    /// A successful charge writes exactly one ledger transaction for the invoice, carrying its
    /// total, its currency, the gateway reference and the payment method, in the same save as the
    /// Paid mark.
    /// </summary>
    [Fact]
    public async Task PayWritesExactlyOneLedgerTransaction()
    {
        var gateway = new FakePaymentGateway { Result = new(true, $"tx-{Guid.NewGuid():N}", null) };
        var admin = await AdminClientAsync(gateway);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory, currency: "AMD");
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 1200m);

        var response = await admin.PostAsync($"/api/billing/{invoiceId}/pay", content: null);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var invoice = await context.Invoices.AsNoTracking().SingleAsync(i => i.Id == invoiceId);
        var rows = await context.Transactions.AsNoTracking().Where(t => t.InvoiceId == invoiceId).ToListAsync();

        rows.Should().ContainSingle();
        rows[0].AmountIn.Should().Be(invoice.Total);
        rows[0].Currency.Should().Be("AMD");
        rows[0].TransactionId.Should().Be(gateway.Result.TransactionId);
        rows[0].PaymentMethod.Should().NotBeNullOrWhiteSpace();
        rows[0].ClientId.Should().Be(clientId);
    }

    /// <summary>Paying an id that names no invoice answers 404 with <c>INVOICE_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task PayRefusesAnUnknownInvoice()
    {
        var admin = await AdminClientAsync(new FakePaymentGateway());

        var response = await admin.PostAsync($"/api/billing/{BillingTestSeeds.NoSuchId}/pay", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await BillingTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.INVOICE_NOT_FOUND);
    }

    /// <summary>A gateway decline answers 502 with <c>GATEWAY_CHARGE_UPSTREAM_FAILED</c> and leaves the invoice unpaid.</summary>
    [Fact]
    public async Task PayRefusesWhenTheGatewayDeclines()
    {
        var gateway = new FakePaymentGateway { Result = new(false, null, "card declined") };
        var admin = await AdminClientAsync(gateway);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 9.99m);

        var response = await admin.PostAsync($"/api/billing/{invoiceId}/pay", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
        (await BillingTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.GATEWAY_CHARGE_UPSTREAM_FAILED);

        var fetched = await admin.GetAsync($"/api/billing/{invoiceId}");
        using var fetchedBody = JsonDocument.Parse(await fetched.Content.ReadAsStringAsync());
        fetchedBody.RootElement.GetProperty("status").GetString().Should().Be(nameof(InvoiceStatus.Unpaid));
    }

    /// <summary>
    /// A gateway that reports success without a transaction id answers 502 with
    /// <c>GATEWAY_TRANSACTION_MISSING_UPSTREAM_FAILED</c>, rather than marking the invoice paid
    /// with nothing to reconcile it against.
    /// </summary>
    [Fact]
    public async Task PayRefusesWhenTheGatewayReportsSuccessWithoutATransactionId()
    {
        var gateway = new FakePaymentGateway { Result = new(true, null, null) };
        var admin = await AdminClientAsync(gateway);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 9.99m);

        var response = await admin.PostAsync($"/api/billing/{invoiceId}/pay", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
        (await BillingTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.GATEWAY_TRANSACTION_MISSING_UPSTREAM_FAILED);
    }

    /// <summary>Charging is not for anonymous callers.</summary>
    [Fact]
    public async Task PayRefusesAnonymous()
    {
        var response = await factory.CreateClient().PostAsync("/api/billing/1/pay", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
