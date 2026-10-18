using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Domain.Support;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Reports;

/// <summary>
/// <c>GET /api/reports/ticket-ratings-reviewer</c> against Postgres: one row per rated reply,
/// newest first.
/// </summary>
/// <remarks>Split from the deleted <c>ReportingRemainderEndpointTests</c> (A7 Task 4).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetTicketRatingsReviewerTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A rated ticket appears in the Ticket Ratings Reviewer report.</summary>
    [Fact]
    public async Task ShowsARatedTicket()
    {
        var marker = $"Review{Guid.NewGuid():N}";
        var clientId = await ReportsTestSeeds.CreateClientAsync(factory, marker);
        int ticketId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var ticket = Ticket.Create(clientId, $"Subject-{marker}", "Body", 1, TicketPriority.Low);
            ticket.LeaveFeedback(9, "All good", marker);
            db.Tickets.Add(ticket);
            await db.SaveChangesAsync();
            ticketId = ticket.Id;
        }

        var client = await ReportsTestSeeds.AdminClientAsync(factory);
        var response = await client.GetAsync("/api/reports/ticket-ratings-reviewer?minRating=9");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("rows").EnumerateArray().Should().Contain(r => r.GetProperty("ticketId").GetInt32() == ticketId);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task RefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/reports/ticket-ratings-reviewer");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client-role caller, not a reseller or admin, is refused with 403.</summary>
    [Fact]
    public async Task RefusesTheClientRole()
    {
        var client = await ReportsTestSeeds.ClientRoleClientAsync(factory, "ticket-ratings-reviewer");

        var response = await client.GetAsync("/api/reports/ticket-ratings-reviewer");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
