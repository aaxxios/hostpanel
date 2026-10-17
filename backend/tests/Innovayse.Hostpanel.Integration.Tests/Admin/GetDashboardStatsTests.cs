using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Admin;

/// <summary>
/// <c>GET /api/admin/dashboard/stats</c>: an admin or reseller reads every counter, including the
/// open-ticket count which reflects only <see cref="TicketStatus.Open"/> tickets, a caller with
/// only the Client role is refused, and the route is closed to a caller with no credential. This
/// use case refuses nothing once authorized.
/// </summary>
/// <remarks>Split out of the deleted <c>DashboardEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetDashboardStatsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Dashboard stats answer 200 with every counter present.</summary>
    [Fact]
    public async Task ReturnsEveryCounterAsync()
    {
        var client = await AdminTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync("/api/admin/dashboard/stats");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("totalRevenue", out _).Should().BeTrue();
        body.TryGetProperty("totalClients", out _).Should().BeTrue();
    }

    /// <summary>
    /// The open ticket count reflects only <see cref="TicketStatus.Open"/> tickets -- the effect of
    /// the dissolved <c>ITicketRepository.CountByStatusAsync</c>, now inlined as
    /// <c>_context.Tickets.CountAsync(t =&gt; t.Status == TicketStatus.Open, ct)</c> in
    /// <c>GetDashboardStatsHandler</c> (A6 Task 6).
    /// </summary>
    [Fact]
    public async Task OpenTicketsCountsOnlyOpenStatusAsync()
    {
        async Task<int> OpenTicketsCountAsync()
        {
            var admin = await AdminTestSeeds.AdminClientAsync(factory);
            var response = await admin.GetAsync("/api/admin/dashboard/stats");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            return body.GetProperty("openTickets").GetInt32();
        }

        var before = await OpenTicketsCountAsync();

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var client = Client.Create($"user-{Guid.NewGuid():N}", "Ada", "Lovelace", $"ada-{Guid.NewGuid():N}@example.test");
        context.Clients.Add(client);
        await context.SaveChangesAsync(CancellationToken.None);
        var openTicket = Ticket.Create(client.Id, "Open one", "Help", departmentId: 1, TicketPriority.Medium);
        var closedTicket = Ticket.Create(client.Id, "Closed one", "Help", departmentId: 1, TicketPriority.Medium);
        closedTicket.Close();
        context.Tickets.Add(openTicket);
        context.Tickets.Add(closedTicket);
        await context.SaveChangesAsync(CancellationToken.None);

        var after = await OpenTicketsCountAsync();

        after.Should().Be(before + 1);
    }

    /// <summary>A caller with only the Client role is refused with 403.</summary>
    [Fact]
    public async Task IsClosedToTheClientRoleAsync()
    {
        var client = await AdminTestSeeds.ClientRoleClientAsync(factory, "dashboard-stats-forbidden");

        var response = await client.GetAsync("/api/admin/dashboard/stats");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>The dashboard is not for anonymous callers.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().GetAsync("/api/admin/dashboard/stats");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
