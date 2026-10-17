using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Clients;

/// <summary><c>POST /api/me/users/invite</c>: a client invites someone to their own account.</summary>
/// <remarks>New coverage: this use case had no test before A7 Task 5.</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class InviteMyUserTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Inviting a fresh address with the portal's permission keys succeeds.</summary>
    [Fact]
    public async Task InviteSendsTheInvitation()
    {
        var (client, _, _) = await ClientsTestSeeds.RegisterClientAsync(factory, "my-invite-happy");

        var response = await client.PostAsJsonAsync("/api/me/users/invite", new
        {
            email = $"invitee-{Guid.NewGuid():N}@example.test",
            firstName = "Invitee",
            lastName = "Person",
            permissions = "contacts,invoices",
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>Inviting the caller's own address answers <c>CANNOT_INVITE_ACCOUNT_OWNER</c>.</summary>
    [Fact]
    public async Task InviteRefusesTheCallersOwnAddress()
    {
        var (client, _, _) = await ClientsTestSeeds.RegisterClientAsync(factory, "my-invite-self");
        var me = await client.GetFromJsonAsync<System.Text.Json.JsonElement>("/api/clients/me");
        var ownEmail = me.GetProperty("email").GetString();

        var response = await client.PostAsJsonAsync("/api/me/users/invite", new
        {
            email = ownEmail,
            firstName = "Invitee",
            lastName = "Person",
            permissions = "contacts",
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await ClientsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.CANNOT_INVITE_ACCOUNT_OWNER);
    }

    /// <summary>Inviting the same address twice answers <c>INVITATION_ALREADY_PENDING</c> the second time.</summary>
    [Fact]
    public async Task InviteRefusesADuplicatePendingInvitation()
    {
        var (client, _, _) = await ClientsTestSeeds.RegisterClientAsync(factory, "my-invite-dupe");
        var inviteeEmail = $"invitee-{Guid.NewGuid():N}@example.test";
        var first = await client.PostAsJsonAsync("/api/me/users/invite", new
        {
            email = inviteeEmail,
            firstName = "Invitee",
            lastName = "Person",
            permissions = "contacts",
        });
        first.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var response = await client.PostAsJsonAsync("/api/me/users/invite", new
        {
            email = inviteeEmail,
            firstName = "Invitee",
            lastName = "Person",
            permissions = "contacts",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        await ClientsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.INVITATION_ALREADY_PENDING);
    }

    /// <summary>Inviting someone to the caller's own account is not for anonymous callers.</summary>
    [Fact]
    public async Task InviteRefusesAnonymous()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/me/users/invite", new
        {
            email = "invitee@example.test",
            firstName = "Invitee",
            lastName = "Person",
            permissions = "contacts",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>An admin, who is not a client, is refused with 403.</summary>
    [Fact]
    public async Task InviteRefusesAnAdmin()
    {
        var client = factory.CreateClient();
        var token = await factory.GetAdminTokenAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/me/users/invite", new
        {
            email = "invitee@example.test",
            firstName = "Invitee",
            lastName = "Person",
            permissions = "contacts",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
