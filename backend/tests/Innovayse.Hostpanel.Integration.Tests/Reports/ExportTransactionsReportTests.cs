using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Reports;

/// <summary>
/// <c>GET /api/reports/transactions/export</c> against Postgres: the transactions report as a
/// CSV file.
/// </summary>
/// <remarks>Split from the deleted <c>ReportingRemainderEndpointTests</c> (A7 Task 4).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ExportTransactionsReportTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>The Transactions CSV export answers with the exact pre-conversion headers, content type and filename.</summary>
    [Fact]
    public async Task ReturnsTheExpectedCsvShape()
    {
        var marker = $"ExportTxn{Guid.NewGuid():N}";
        var clientId = await ReportsTestSeeds.CreateClientAsync(factory, marker);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var tx = Transaction.Create(
                clientId, DateTimeOffset.UtcNow, "Export test", $"tx-{Guid.NewGuid():N}",
                invoiceId: null, paymentMethod: "manual", amountIn: 12m, amountOut: 0m, fees: 0m, addedToCredit: false);
            db.Transactions.Add(tx);
            await db.SaveChangesAsync();
        }

        var client = await ReportsTestSeeds.AdminClientAsync(factory);
        var response = await client.GetAsync("/api/reports/transactions/export");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
        response.Content.Headers.ContentDisposition!.FileName.Should().Be("transactions-report.csv");
        var csv = await response.Content.ReadAsStringAsync();
        csv.Should().StartWith(
            "ID,Client ID,Client Name,Currency,Payment Method,Date,Description,Invoice ID,Transaction ID,Amount In,Fees,Amount Out");
        csv.Should().Contain(marker);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task RefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/reports/transactions/export");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client-role caller, not a reseller or admin, is refused with 403.</summary>
    [Fact]
    public async Task RefusesTheClientRole()
    {
        var client = await ReportsTestSeeds.ClientRoleClientAsync(factory, "export-transactions-report");

        var response = await client.GetAsync("/api/reports/transactions/export");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
