using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>POST /api/tickets/{id}/toggle-flag</c>: staff can toggle a ticket's flagged state, an id
/// names no ticket is refused <c>TICKET_NOT_FOUND</c>, and the action is closed to a caller with
/// no credential.
/// </summary>
/// <remarks>Split out of the deleted <c>TicketsAdminEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ToggleTicketFlagTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Staff can toggle the flagged state of a ticket.</summary>
    [Fact]
    public async Task AdminCanToggleTheFlagAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);
        var id = await SupportTestSeeds.CreateAdminTicketAsync(client);

        var response = await client.PostAsync($"/api/tickets/{id}/toggle-flag", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>Toggling the flag on a ticket that does not exist answers 404 with <c>TICKET_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task ToggleFlagOnUnknownTicketAnswersNotFoundAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);

        var response = await client.PostAsync($"/api/tickets/{SupportTestSeeds.NoSuchId}/toggle-flag", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await SupportTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.TICKET_NOT_FOUND);
    }

    /// <summary>A caller with no credential cannot toggle a ticket's flag.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().PostAsync(
            $"/api/tickets/{SupportTestSeeds.NoSuchId}/toggle-flag", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
