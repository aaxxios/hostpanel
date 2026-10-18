using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Domain.Billing;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Reports;

/// <summary>
/// <c>GET /api/reports/client-statement</c> against Postgres: a client's running balance of
/// invoices and transactions.
/// </summary>
/// <remarks>Split from the deleted <c>ReportingEndpointTests</c> (A7 Task 4).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ClientStatementTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A known client's statement includes their invoice as a debit line.</summary>
    [Fact]
    public async Task ShowsAKnownClientsInvoice()
    {
        var clientId = await ReportsTestSeeds.CreateClientAsync(factory, $"Statement{Guid.NewGuid():N}");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var invoice = Invoice.Create(clientId, DateTimeOffset.UtcNow.AddDays(7), "USD");
            invoice.AddItem("Domain", 15m, 1);
            db.Invoices.Add(invoice);
            await db.SaveChangesAsync();
        }

        var client = await ReportsTestSeeds.AdminClientAsync(factory);
        var response = await client.GetAsync($"/api/reports/client-statement?clientId={clientId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("clientId").GetInt32().Should().Be(clientId);
        body.GetProperty("lines").EnumerateArray().Should().Contain(l => l.GetProperty("debit").GetDecimal() == 15m);
    }

    /// <summary>An id naming nobody is refused as CLIENT_NOT_FOUND, not a 500.</summary>
    [Fact]
    public async Task RefusesAnUnknownClient()
    {
        var client = await ReportsTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync("/api/reports/client-statement?clientId=999999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await ReportsTestSeeds.CodeOfAsync(response)).Should().Be(HostpanelErrorCodes.CLIENT_NOT_FOUND);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task RefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/reports/client-statement?clientId=1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client-role caller, not a reseller or admin, is refused with 403.</summary>
    [Fact]
    public async Task RefusesTheClientRole()
    {
        var client = await ReportsTestSeeds.ClientRoleClientAsync(factory, "client-statement");

        var response = await client.GetAsync("/api/reports/client-statement?clientId=1");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
