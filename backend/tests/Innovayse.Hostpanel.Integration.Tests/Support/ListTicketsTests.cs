using System.Net;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>GET /api/tickets</c>: staff can list every support ticket, and a plain client is refused --
/// the route carries the class-level <c>ResellerOrAdmin</c> policy.
/// </summary>
/// <remarks>Split out of the deleted <c>SupportEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ListTicketsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Staff can list every support ticket.</summary>
    [Fact]
    public async Task AdminCanListTicketsAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);

        var response = await client.GetAsync("/api/tickets");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>A registered client, who is neither a reseller nor an admin, is forbidden.</summary>
    [Fact]
    public async Task ClientIsForbiddenAsync()
    {
        var (client, _) = await SupportTestSeeds.RegisterClientAsync(factory, "list-tickets-client");

        var response = await client.GetAsync("/api/tickets");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>A caller with no credential cannot list tickets.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().GetAsync("/api/tickets");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>The staff list names the client who opened each ticket, so it does not have to fall back to a bare client number.</summary>
    [Fact]
    public async Task ListRowsNameTheClientWhoOpenedTheTicketAsync()
    {
        var admin = await SupportTestSeeds.AdminClientAsync(factory);
        int ticketId;
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
            var client = Client.Create($"user-{Guid.NewGuid():N}", "Grace", "Hopper", $"grace-{Guid.NewGuid():N}@example.test");
            context.Clients.Add(client);
            await context.SaveChangesAsync(CancellationToken.None);
            var ticket = Ticket.Create(client.Id, "Named requestor", "Hello", departmentId: 1, TicketPriority.Medium);
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync(CancellationToken.None);
            ticketId = ticket.Id;
        }

        var response = await admin.GetAsync("/api/tickets?page=1&pageSize=100");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var row = body.RootElement.GetProperty("items").EnumerateArray().Single(t => t.GetProperty("id").GetInt32() == ticketId);
        row.GetProperty("clientName").GetString().Should().Be("Grace Hopper");
    }
}
