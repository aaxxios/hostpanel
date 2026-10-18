using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Billing.Commands.BulkInvoiceAction;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>POST /api/billing/bulk</c> against Postgres: acting on many invoices at once, the
/// validation-time refusal for an unrecognised action, the handler's own defensive answer for the
/// same, and the count a <c>MarkPaid</c> action reports for eligible rows.
/// </summary>
/// <remarks>Split from the deleted <c>InvoicePaymentsEndpointTests</c> (A7 Task 3).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class BulkInvoiceActionTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// The API route refuses an unrecognised action name at validation, before the handler's own
    /// defensive <c>BULK_INVOICE_ACTION_UNKNOWN</c> arm would ever run.
    /// </summary>
    [Fact]
    public async Task BulkActionRefusesAnUnknownActionNameAtValidation()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 19.99m);

        var response = await admin.PostAsJsonAsync("/api/billing/bulk", new
        {
            invoiceIds = new[] { invoiceId },
            action = "NotARealAction",
        });

        response.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    /// <summary>
    /// <see cref="BulkInvoiceActionHandler"/>'s own defensive answer for an action name that is not
    /// one of the ones it switches on — reachable only by calling the handler directly, since the
    /// command's validator refuses the same input first on the real API path.
    /// </summary>
    [Fact]
    public async Task HandlerAnswersBulkInvoiceActionUnknownForAnUnrecognisedActionName()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 19.99m);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var handler = new BulkInvoiceActionHandler(context);

        var outcome = await handler.HandleAsync(
            new BulkInvoiceActionCommand([invoiceId], "NotARealAction"), CancellationToken.None);

        outcome.IsFailure.Should().BeTrue();
        outcome.ErrorCode.Should().Be(HostpanelErrorCodes.BULK_INVOICE_ACTION_UNKNOWN);
    }

    /// <summary>A <c>MarkPaid</c> bulk action against a Draft invoice affects nothing and answers zero.</summary>
    [Fact]
    public async Task BulkActionMarkPaidCountsOnlyEligibleInvoices()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var draftInvoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 9.99m, isDraft: true);

        var response = await admin.PostAsJsonAsync("/api/billing/bulk", new
        {
            invoiceIds = new[] { draftInvoiceId },
            action = "MarkPaid",
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var affected = await response.Content.ReadFromJsonAsync<int>();
        affected.Should().Be(0);
    }

    /// <summary>A <c>MarkPaid</c> bulk action against an unpaid invoice marks it paid and counts it.</summary>
    [Fact]
    public async Task BulkActionMarkPaidMarksAnUnpaidInvoicePaid()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 9.99m);

        var response = await admin.PostAsJsonAsync("/api/billing/bulk", new
        {
            invoiceIds = new[] { invoiceId },
            action = "MarkPaid",
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var affected = await response.Content.ReadFromJsonAsync<int>();
        affected.Should().Be(1);
    }

    /// <summary>
    /// A <c>MarkPaid</c> bulk action writes exactly one ledger transaction per settled invoice,
    /// with the invoice total, currency and the manual payment method.
    /// </summary>
    [Fact]
    public async Task BulkActionMarkPaidWritesOneLedgerTransactionPerInvoice()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory, currency: "AMD");
        var first = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 500m);
        var second = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 700m);

        var response = await admin.PostAsJsonAsync("/api/billing/bulk", new
        {
            invoiceIds = new[] { first, second },
            action = "MarkPaid",
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        foreach (var invoiceId in new[] { first, second })
        {
            var invoice = await context.Invoices.AsNoTracking().SingleAsync(i => i.Id == invoiceId);
            var rows = await context.Transactions.AsNoTracking().Where(t => t.InvoiceId == invoiceId).ToListAsync();

            rows.Should().ContainSingle();
            rows[0].AmountIn.Should().Be(invoice.Total);
            rows[0].Currency.Should().Be("AMD");
            rows[0].PaymentMethod.Should().Be(PaymentMethodNames.Manual);
        }
    }

    /// <summary>The bulk action endpoint is not for anonymous callers.</summary>
    [Fact]
    public async Task BulkActionRefusesAnonymous()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/billing/bulk", new
        {
            invoiceIds = new[] { 1 },
            action = "MarkPaid",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
