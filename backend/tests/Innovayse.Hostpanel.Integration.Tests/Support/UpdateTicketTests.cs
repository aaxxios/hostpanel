using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Domain.Support.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Support;

/// <summary>
/// <c>PUT /api/tickets/{id}</c>: an admin can update a ticket's metadata, an id names no ticket
/// is refused <c>TICKET_NOT_FOUND</c> (the pre-conversion route answered 400), and the action is
/// closed to a caller with no credential.
/// </summary>
/// <remarks>Split out of the deleted <c>TicketsAdminEndpointTests</c>.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class UpdateTicketTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>An admin can update a ticket's status.</summary>
    [Fact]
    public async Task AdminCanUpdateATicketAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);
        var id = await SupportTestSeeds.CreateAdminTicketAsync(client);

        var response = await client.PutAsJsonAsync($"/api/tickets/{id}", new { status = nameof(TicketStatus.Closed) });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>Updating a ticket that does not exist answers 404 with <c>TICKET_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task UpdateUnknownTicketAnswersNotFoundAsync()
    {
        var client = await SupportTestSeeds.AdminClientAsync(factory);

        var response = await client.PutAsJsonAsync($"/api/tickets/{SupportTestSeeds.NoSuchId}", new { status = nameof(TicketStatus.Closed) });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await SupportTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.TICKET_NOT_FOUND);
    }

    /// <summary>A caller with no credential cannot update a ticket.</summary>
    [Fact]
    public async Task IsClosedToAnonymousCallersAsync()
    {
        var response = await factory.CreateClient().PutAsJsonAsync(
            $"/api/tickets/{SupportTestSeeds.NoSuchId}", new { status = nameof(TicketStatus.Closed) });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
