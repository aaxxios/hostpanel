using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Billing;

/// <summary>
/// <c>GET /api/billing</c> against Postgres: the admin-wide, paginated invoice list.
/// </summary>
/// <remarks>Split from the deleted <c>BillingEndpointTests</c> (A7 Task 3).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ListInvoicesTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Listing invoices is not for anonymous callers.</summary>
    [Fact]
    public async Task GetInvoicesAsync_Returns401_WithoutAuthAsync()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/billing");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>An admin lists invoices and sees the page.</summary>
    [Fact]
    public async Task GetInvoicesAsync_Returns200_AsAdminAsync()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);

        var response = await admin.GetAsync("/api/billing");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>An invoice just created for a client appears in the admin's list.</summary>
    [Fact]
    public async Task ListIncludesAFreshlyCreatedInvoice()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        var invoiceId = await BillingTestSeeds.CreateInvoiceAsync(admin, clientId, 5m);

        var response = await admin.GetAsync("/api/billing?page=1&pageSize=100");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("items").EnumerateArray()
            .Select(i => i.GetProperty("id").GetInt32()).Should().Contain(invoiceId);
    }

    /// <summary>Every row of the list names the currency its amounts are in, so a list can print a figure with the right unit.</summary>
    [Fact]
    public async Task ListRowsCarryTheInvoiceCurrencyAsync()
    {
        var admin = await BillingTestSeeds.AdminClientAsync(factory);
        var clientId = await BillingTestSeeds.SeedClientAsync(factory);
        int invoiceId;
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
            var invoice = Invoice.Create(clientId, DateTimeOffset.UtcNow.AddDays(14), "EUR");
            invoice.AddItem("Hosting", 12m, 1, null);
            context.Invoices.Add(invoice);
            await context.SaveChangesAsync(CancellationToken.None);
            invoiceId = invoice.Id;
        }

        var response = await admin.GetAsync("/api/billing?page=1&pageSize=100");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var row = body.RootElement.GetProperty("items").EnumerateArray().Single(i => i.GetProperty("id").GetInt32() == invoiceId);
        row.GetProperty("currency").GetString().Should().Be("EUR");
    }
}
