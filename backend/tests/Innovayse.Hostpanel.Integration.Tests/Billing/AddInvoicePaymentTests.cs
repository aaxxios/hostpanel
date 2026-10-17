using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Domain.Billing.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>POST /api/billing/{id}/payment</c> against Postgres: recording a manual payment marks the
/// invoice paid and writes an activity log entry, and the <c>INVOICE_NOT_FOUND</c> refusal for an
/// id that names no invoice.
/// </summary>
/// <remarks>Split from the deleted <c>InvoicePaymentsEndpointTests</c> (A7 Task 3).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class AddInvoicePaymentTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Recording a manual payment against an invoice marks it paid.</summary>
    [Fact]
    public async Task AddPaymentMarksTheInvoicePaid()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 19.99m);

        var response = await admin.PostAsJsonAsync($"/api/billing/{invoiceId}/payment", new
        {
            date = DateTimeOffset.UtcNow,
            gateway = PaymentMethodNames.Manual,
            transactionId = $"pay-{Guid.NewGuid():N}",
            amount = 19.99m,
            fees = 0m,
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var fetched = await admin.GetAsync($"/api/billing/{invoiceId}");
        using var body = JsonDocument.Parse(await fetched.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("status").GetString().Should().Be(nameof(InvoiceStatus.Paid));
    }

    /// <summary>
    /// Recording a manual payment writes an activity log entry against the invoice's client --
    /// the effect of the dissolved <c>IActivityLogRepository.Add</c>, now <c>_context.ActivityLogs.Add</c>
    /// directly in <c>AddInvoicePaymentHandler</c> (A6 Task 6).
    /// </summary>
    [Fact]
    public async Task AddPayment_RecordsAnActivityLogEntryForTheInvoicesClientAsync()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 19.99m);

        var response = await admin.PostAsJsonAsync($"/api/billing/{invoiceId}/payment", new
        {
            date = DateTimeOffset.UtcNow,
            gateway = PaymentMethodNames.Manual,
            transactionId = $"pay-{Guid.NewGuid():N}",
            amount = 19.99m,
            fees = 0m,
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        // CreateInvoiceHandler also writes an activity log entry for the same client (an "Invoice
        // Created" row), so the payment's own entry is matched by its description, not by client alone.
        var log = await context.ActivityLogs.SingleOrDefaultAsync(
            l => l.ClientId == clientId && l.Description.Contains("Added Invoice Payment"));
        log.Should().NotBeNull();
        log!.Description.Should().Contain($"Invoice ID: {invoiceId}");
    }

    /// <summary>Recording a payment against an id that names no invoice answers 404 with <c>INVOICE_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task AddPaymentRefusesAnUnknownInvoice()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);

        var response = await admin.PostAsJsonAsync($"/api/billing/{BillingTestSeeds.NoSuchId}/payment", new
        {
            date = DateTimeOffset.UtcNow,
            gateway = PaymentMethodNames.Manual,
            transactionId = $"pay-{Guid.NewGuid():N}",
            amount = 10m,
            fees = 0m,
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await BillingTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.INVOICE_NOT_FOUND);
    }

    /// <summary>Recording a payment is not for anonymous callers.</summary>
    [Fact]
    public async Task AddPaymentRefusesAnonymous()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/billing/1/payment", new
        {
            date = DateTimeOffset.UtcNow,
            gateway = PaymentMethodNames.Manual,
            transactionId = "ext",
            amount = 10m,
            fees = 0m,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
