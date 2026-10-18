using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Clients;

/// <summary>
/// <c>POST /api/clients/admin</c>: creating a client account from the admin panel, either by
/// provisioning a new account or by linking an existing one.
/// </summary>
/// <remarks>Split from the deleted <c>ClientsAdminEndpointTests</c> (A7 Task 5).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class AdminCreateClientTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Linking a client account to an existing, unlinked Identity subject succeeds.</summary>
    [Fact]
    public async Task LinksAnExistingSubject()
    {
        var (userId, _) = await ClientsTestSeeds.CreateUnlinkedSubjectAsync(factory, "admin-create-happy");
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);

        var created = await admin.PostAsJsonAsync("/api/clients/admin", ClientsTestSeeds.AdminCreateBody(existingUserId: userId));

        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var clientId = await created.Content.ReadFromJsonAsync<int>();

        var fetched = await admin.GetAsync($"/api/clients/{clientId}");
        fetched.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await fetched.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("lastName").GetString().Should().Be("Client");
    }

    /// <summary>Linking a client account to a subject that already has one answers <c>EXISTING_USER_ALREADY_HAS_CLIENT</c>.</summary>
    [Fact]
    public async Task RefusesASubjectThatAlreadyHasAClient()
    {
        var (userId, _) = await ClientsTestSeeds.RegisterRealClientAsync(factory, "admin-create-dupe");
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);

        var response = await admin.PostAsJsonAsync("/api/clients/admin", ClientsTestSeeds.AdminCreateBody(existingUserId: userId));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        await ClientsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.EXISTING_USER_ALREADY_HAS_CLIENT);
    }

    /// <summary>Linking a nonexistent existing-user id answers <c>IDENTITY_USER_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task RefusesAnUnknownExistingUser()
    {
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);

        var response = await admin.PostAsJsonAsync(
            "/api/clients/admin", ClientsTestSeeds.AdminCreateBody(existingUserId: ClientsTestSeeds.NoSuchUserId));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await ClientsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.IDENTITY_USER_NOT_FOUND);
    }

    /// <summary>An unconfigured currency is refused as missing, even on the admin-create path.</summary>
    [Fact]
    public async Task RefusesAnUnconfiguredCurrency()
    {
        var (userId, _) = await ClientsTestSeeds.CreateUnlinkedSubjectAsync(factory, "admin-create-currency");
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);

        var response = await admin.PostAsJsonAsync(
            "/api/clients/admin", ClientsTestSeeds.AdminCreateBody(existingUserId: userId, currency: "ZZZ"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await ClientsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.CURRENCY_NOT_FOUND);
    }

    /// <summary>Admin-creating a client is not for anonymous callers.</summary>
    [Fact]
    public async Task RefusesAnonymous()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(
            "/api/clients/admin", ClientsTestSeeds.AdminCreateBody(existingUserId: ClientsTestSeeds.NoSuchUserId));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client, who is not staff, is refused with 403.</summary>
    [Fact]
    public async Task RefusesAClient()
    {
        var (client, _, _) = await ClientsTestSeeds.RegisterClientAsync(factory, "admin-create-client-forbidden");

        var response = await client.PostAsJsonAsync(
            "/api/clients/admin", ClientsTestSeeds.AdminCreateBody(existingUserId: ClientsTestSeeds.NoSuchUserId));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
