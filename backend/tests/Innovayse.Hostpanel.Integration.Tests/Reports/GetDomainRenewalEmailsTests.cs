using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Reports;

/// <summary>
/// <c>GET /api/reports/domain-renewal-emails</c> against Postgres: every matching renewal
/// reminder, most recently sent first.
/// </summary>
/// <remarks>Split from the deleted <c>ReportingEndpointTests</c> (A7 Task 4).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetDomainRenewalEmailsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A sent renewal reminder appears in the report, scoped to its client.</summary>
    [Fact]
    public async Task ShowsASentReminder()
    {
        var marker = $"Renew{Guid.NewGuid():N}";
        var clientId = await ReportsTestSeeds.CreateClientAsync(factory, marker);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var domain = RegisteredDomain.Register(
                clientId, $"{marker}.com", DateTimeOffset.UtcNow.AddDays(30), autoRenew: false, whoisPrivacy: false);
            domain.AddReminder("30 Days Before Expiry", $"{marker}@reports.test");
            db.Domains.Add(domain);
            await db.SaveChangesAsync();
        }

        var client = await ReportsTestSeeds.AdminClientAsync(factory);
        var response = await client.GetAsync($"/api/reports/domain-renewal-emails?clientId={clientId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("rows").EnumerateArray().Should().Contain(
            r => r.GetProperty("domain").GetString() == $"{marker}.com");
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task RefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/reports/domain-renewal-emails");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client-role caller, not a reseller or admin, is refused with 403.</summary>
    [Fact]
    public async Task RefusesTheClientRole()
    {
        var client = await ReportsTestSeeds.ClientRoleClientAsync(factory, "domain-renewal-emails");

        var response = await client.GetAsync("/api/reports/domain-renewal-emails");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
