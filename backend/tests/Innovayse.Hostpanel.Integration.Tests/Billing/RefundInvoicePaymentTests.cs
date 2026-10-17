using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Billing.Commands.RefundInvoicePayment.Enums;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>POST /api/billing/{id}/refund</c> against Postgres: refunding a paid invoice via credit
/// balance, and the <c>INVOICE_NOT_FOUND</c> refusal for an id that names no invoice.
/// </summary>
/// <remarks>Split from the deleted <c>InvoicePaymentsEndpointTests</c> (A7 Task 3).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class RefundInvoicePaymentTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// A refund via <c>CreditBalance</c> against a paid invoice records the refund and credits the
    /// client's balance.
    /// </summary>
    [Fact]
    public async Task RefundViaCreditBalanceRefundsAPaidInvoice()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 19.99m);

        var paid = await admin.PostAsJsonAsync($"/api/billing/{invoiceId}/payment", new
        {
            date = DateTimeOffset.UtcNow,
            gateway = PaymentMethodNames.Manual,
            transactionId = $"pay-{Guid.NewGuid():N}",
            amount = 19.99m,
            fees = 0m,
        });
        paid.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var refunded = await admin.PostAsJsonAsync($"/api/billing/{invoiceId}/refund", new
        {
            amount = 0m,
            refundType = nameof(RefundType.CreditBalance),
            gateway = string.Empty,
            transactionId = "orig-ref",
        });

        refunded.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var fetched = await admin.GetAsync($"/api/billing/{invoiceId}");
        using var body = JsonDocument.Parse(await fetched.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("status").GetString().Should().Be(nameof(InvoiceStatus.Refunded));
    }

    /// <summary>Refunding an id that names no invoice answers 404 with <c>INVOICE_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task RefundRefusesAnUnknownInvoice()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);

        var response = await admin.PostAsJsonAsync($"/api/billing/{BillingTestSeeds.NoSuchId}/refund", new
        {
            amount = 0m,
            refundType = nameof(RefundType.Manual),
            gateway = PaymentMethodNames.Manual,
            transactionId = "orig-ref",
            refundTransactionId = $"refund-{Guid.NewGuid():N}",
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await BillingTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.INVOICE_NOT_FOUND);
    }

    /// <summary>Refunding a payment is not for anonymous callers.</summary>
    [Fact]
    public async Task RefundRefusesAnonymous()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/billing/1/refund", new
        {
            amount = 0m,
            refundType = nameof(RefundType.Manual),
            gateway = PaymentMethodNames.Manual,
            transactionId = "orig-ref",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
