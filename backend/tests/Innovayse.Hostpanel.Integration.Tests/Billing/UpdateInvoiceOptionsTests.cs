using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>PUT /api/billing/{id}/options</c> against Postgres: writing an invoice's dates, payment
/// method and tax rate, and the <c>INVOICE_NOT_FOUND</c> refusal for an id that names no invoice.
/// </summary>
/// <remarks>Split from the deleted <c>InvoicesEndpointTests</c> (A7 Task 3).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class UpdateInvoiceOptionsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Writing a tax rate recomputes the invoice's total.</summary>
    [Fact]
    public async Task UpdateOptionsRecomputesTheTotalFromTheTaxRate()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 100m);

        var response = await admin.PutAsJsonAsync($"/api/billing/{invoiceId}/options", new
        {
            invoiceDate = DateTimeOffset.UtcNow,
            dueDate = DateTimeOffset.UtcNow.AddDays(7),
            paymentMethod = PaymentMethodNames.Manual,
            taxRate = 10m,
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var fetched = await admin.GetAsync($"/api/billing/{invoiceId}");
        using var body = JsonDocument.Parse(await fetched.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("total").GetDecimal().Should().Be(110m);
        body.RootElement.GetProperty("paymentMethod").GetString().Should().Be(PaymentMethodNames.Manual);
    }

    /// <summary>Updating options on an id that names no invoice answers 404, not 400.</summary>
    [Fact]
    public async Task UpdateOptionsOnUnknownInvoiceRefusesNotFound()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);

        var response = await admin.PutAsJsonAsync($"/api/billing/{BillingTestSeeds.NoSuchId}/options", new
        {
            invoiceDate = DateTimeOffset.UtcNow,
            dueDate = DateTimeOffset.UtcNow.AddDays(7),
            paymentMethod = (string?)null,
            taxRate = 0m,
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await BillingTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.INVOICE_NOT_FOUND);
    }

    /// <summary>Updating options is not for anonymous callers.</summary>
    [Fact]
    public async Task UpdateOptionsRefusesAnonymous()
    {
        var response = await factory.CreateClient().PutAsJsonAsync("/api/billing/1/options", new
        {
            invoiceDate = DateTimeOffset.UtcNow,
            dueDate = DateTimeOffset.UtcNow.AddDays(7),
            paymentMethod = (string?)null,
            taxRate = 0m,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
