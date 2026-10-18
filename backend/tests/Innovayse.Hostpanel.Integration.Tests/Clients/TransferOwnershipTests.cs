using System.Net;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Clients;

/// <summary><c>POST /api/clients/{id}/users/{userId}/make-owner</c>: transferring account ownership.</summary>
/// <remarks>Split from the deleted <c>ClientsAdminEndpointTests</c> (A7 Task 5).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class TransferOwnershipTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Transferring ownership to a different user succeeds.</summary>
    [Fact]
    public async Task TransferWritesTheNewOwner()
    {
        var (clientId, _) = await ClientsTestSeeds.SeedClientAsync(factory);
        var (newOwnerUserId, _) = await ClientsTestSeeds.CreateUnlinkedSubjectAsync(factory, "transfer-happy");
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);

        var response = await admin.PostAsync($"/api/clients/{clientId}/users/{newOwnerUserId}/make-owner", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    /// <summary>Transferring ownership to a user who already owns another client answers 409, not a database error.</summary>
    [Fact]
    public async Task TransferRefusesAUserWhoAlreadyOwnsAnotherClient()
    {
        var (clientId, _) = await ClientsTestSeeds.SeedClientAsync(factory);
        var (existingOwnerUserId, _) = await ClientsTestSeeds.RegisterRealClientAsync(factory, "transfer-dupe");
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);

        var response = await admin.PostAsync($"/api/clients/{clientId}/users/{existingOwnerUserId}/make-owner", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        await ClientsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.EXISTING_USER_ALREADY_HAS_CLIENT);
    }

    /// <summary>Transferring ownership on an id that names no client answers 404 with <c>CLIENT_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task TransferRefusesUnknownClient()
    {
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);

        var response = await admin.PostAsync(
            $"/api/clients/{ClientsTestSeeds.NoSuchClientId}/users/{ClientsTestSeeds.NoSuchUserId}/make-owner", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await ClientsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.CLIENT_NOT_FOUND);
    }

    /// <summary>Transferring ownership is not for anonymous callers.</summary>
    [Fact]
    public async Task TransferRefusesAnonymous()
    {
        var response = await factory.CreateClient().PostAsync(
            $"/api/clients/{ClientsTestSeeds.NoSuchClientId}/users/{ClientsTestSeeds.NoSuchUserId}/make-owner", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client, who is not staff, is refused with 403.</summary>
    [Fact]
    public async Task TransferRefusesAClient()
    {
        var (client, clientId, _) = await ClientsTestSeeds.RegisterClientAsync(factory, "transfer-forbidden");

        var response = await client.PostAsync(
            $"/api/clients/{clientId}/users/{ClientsTestSeeds.NoSuchUserId}/make-owner", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
