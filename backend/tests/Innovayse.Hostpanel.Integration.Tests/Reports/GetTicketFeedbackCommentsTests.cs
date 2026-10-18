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
/// <c>GET /api/reports/ticket-feedback-comments</c> against Postgres: one row per piece of
/// feedback, newest first.
/// </summary>
/// <remarks>Split from the deleted <c>ReportingRemainderEndpointTests</c> (A7 Task 4).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetTicketFeedbackCommentsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>A rated ticket's feedback appears in the Ticket Feedback Comments report with the staff placeholder name.</summary>
    [Fact]
    public async Task ShowsARatedTicket()
    {
        var marker = $"Feedback{Guid.NewGuid():N}";
        var clientId = await ReportsTestSeeds.CreateClientAsync(factory, marker);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var ticket = Ticket.Create(clientId, $"Subject-{marker}", "Body", 1, TicketPriority.Low);
            ticket.Assign(77);
            ticket.LeaveFeedback(8, "Great support", marker);
            db.Tickets.Add(ticket);
            await db.SaveChangesAsync();
        }

        var client = await ReportsTestSeeds.AdminClientAsync(factory);
        var response = await client.GetAsync("/api/reports/ticket-feedback-comments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("rows").EnumerateArray().Should().Contain(
            r => r.GetProperty("feedbackLeftBy").GetString() == marker
                && r.GetProperty("staffName").GetString() == "Staff #77"
                && r.GetProperty("rating").GetInt32() == 8);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task RefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync("/api/reports/ticket-feedback-comments");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client-role caller, not a reseller or admin, is refused with 403.</summary>
    [Fact]
    public async Task RefusesTheClientRole()
    {
        var client = await ReportsTestSeeds.ClientRoleClientAsync(factory, "ticket-feedback-comments");

        var response = await client.GetAsync("/api/reports/ticket-feedback-comments");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
