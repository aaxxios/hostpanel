using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Integration.Tests.Reports;

/// <summary>
/// <c>GET /api/reports/sales-tax</c> against Postgres: the invoices paid in a range of days, with
/// the liability totalled per invoice currency and the rows paged.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetSalesTaxReportTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A paid invoice with tax is listed with its currency.</summary>
    [Fact]
    public async Task IncludesAPaidTaxedInvoice()
    {
        var marker = $"Tax{Guid.NewGuid():N}";
        var clientId = await ReportsTestSeeds.CreateClientAsync(factory, marker);
        await ReportsTestSeeds.CreatePaidInvoiceAsync(factory, clientId, 10m, 100m);

        var client = await ReportsTestSeeds.AdminClientAsync(factory);
        var response = await client.GetAsync("/api/reports/sales-tax?pageSize=200");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("rows").GetProperty("items").EnumerateArray().Should().Contain(
            r => r.GetProperty("clientName").GetString()!.Contains(marker)
                && r.GetProperty("tax").GetDecimal() == 10m
                && r.GetProperty("currency").GetString() == "USD");
    }

    /// <summary>
    /// The range is matched on the day the invoice was paid, not the day it was raised, and the
    /// totals come per currency, never added across currencies.
    /// </summary>
    [Fact]
    public async Task MatchesThePaymentDayAndTotalsPerCurrency()
    {
        var clientId = await ReportsTestSeeds.CreateClientAsync(factory, $"TaxDay{Guid.NewGuid():N}");
        var paidAt = new DateTimeOffset(2022, 5, 10, 23, 30, 0, TimeSpan.Zero);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            foreach (var (currency, amount) in new[] { ("SEK", 100m), ("SEK", 50m), ("NOK", 80m) })
            {
                var invoice = Invoice.Create(clientId, DateTimeOffset.UtcNow.AddDays(7), currency, DateTimeOffset.UtcNow);
                invoice.AddItem("Hosting", amount, 1);
                invoice.UpdateOptions(invoice.InvoiceDate, invoice.DueDate, null, 10m);
                invoice.RecordPaidHistory($"tx-{Guid.NewGuid():N}", paidAt);
                db.Invoices.Add(invoice);
            }

            await db.SaveChangesAsync();
        }

        var client = await ReportsTestSeeds.AdminClientAsync(factory);
        var response = await client.GetAsync("/api/reports/sales-tax?from=2022-05-10&to=2022-05-10&page=1&pageSize=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var totals = body.GetProperty("totals").EnumerateArray().ToList();
        var sek = totals.Single(t => t.GetProperty("currency").GetString() == "SEK");
        sek.GetProperty("totalInvoices").GetInt32().Should().Be(2);
        sek.GetProperty("totalInvoiced").GetDecimal().Should().Be(165m);
        sek.GetProperty("taxLiability").GetDecimal().Should().Be(15m);
        totals.Single(t => t.GetProperty("currency").GetString() == "NOK").GetProperty("taxLiability").GetDecimal().Should().Be(8m);
        var rows = body.GetProperty("rows");
        rows.GetProperty("items").GetArrayLength().Should().Be(2);
        rows.GetProperty("totalCount").GetInt32().Should().Be(3);
        rows.GetProperty("items")[0].GetProperty("datePaid").GetString().Should().Be("2022-05-10");
    }

    /// <summary>A page size above the report ceiling is refused by the validator.</summary>
    [Fact]
    public async Task RefusesAnOversizedPage()
    {
        var client = await ReportsTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync("/api/reports/sales-tax?pageSize=100000");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task RefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/reports/sales-tax");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client-role caller, not an admin, is refused with 403.</summary>
    [Fact]
    public async Task RefusesTheClientRole()
    {
        var client = await ReportsTestSeeds.ClientRoleClientAsync(factory, "sales-tax");

        var response = await client.GetAsync("/api/reports/sales-tax");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
