using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Reports;

/// <summary>
/// <c>GET /api/reports/credits-reviewer</c> against Postgres: every matching credit, newest first.
/// </summary>
/// <remarks>Split from the deleted <c>ReportingEndpointTests</c> (A7 Task 4).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetCreditsReviewerTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A transaction added to credit appears in the credits reviewer, scoped to its client.</summary>
    [Fact]
    public async Task ShowsACreditedTransaction()
    {
        var clientId = await ReportsTestSeeds.CreateClientAsync(factory, $"Credit{Guid.NewGuid():N}");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var tx = Transaction.Create(
                clientId, DateTimeOffset.UtcNow, "Refund credit", $"tx-{Guid.NewGuid():N}",
                invoiceId: null, paymentMethod: "manual", amountIn: 25m, amountOut: 0m, fees: 0m, addedToCredit: true, currency: "USD");
            db.Transactions.Add(tx);
            await db.SaveChangesAsync();
        }

        var client = await ReportsTestSeeds.AdminClientAsync(factory);
        var response = await client.GetAsync($"/api/reports/credits-reviewer?clientId={clientId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("totalCount").GetInt32().Should().Be(1);
        body.GetProperty("rows")[0].GetProperty("amount").GetDecimal().Should().Be(25m);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task RefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/reports/credits-reviewer");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client-role caller, not a reseller or admin, is refused with 403.</summary>
    [Fact]
    public async Task RefusesTheClientRole()
    {
        var client = await ReportsTestSeeds.ClientRoleClientAsync(factory, "credits-reviewer");

        var response = await client.GetAsync("/api/reports/credits-reviewer");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
