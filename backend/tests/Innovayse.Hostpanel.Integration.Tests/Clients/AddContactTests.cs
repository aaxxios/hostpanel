using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Clients;

/// <summary><c>POST /api/clients/{id}/contacts</c>: adding an additional contact to a client account.</summary>
/// <remarks>Split from the deleted <c>ClientsAdminEndpointTests</c> (A7 Task 5).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class AddContactTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Adding a contact writes it onto the client's record.</summary>
    [Fact]
    public async Task AddWritesTheContact()
    {
        var (clientId, _) = await ClientsTestSeeds.SeedClientAsync(factory);
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);

        var response = await admin.PostAsJsonAsync($"/api/clients/{clientId}/contacts", ClientsTestSeeds.ContactBody("Billing"));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var fetched = await admin.GetFromJsonAsync<JsonElement>($"/api/clients/{clientId}");
        fetched.GetProperty("contacts").EnumerateArray().Single().GetProperty("firstName").GetString().Should().Be("Billing");
    }

    /// <summary>Adding a contact to an id that names no client answers 404 with <c>CLIENT_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task AddRefusesUnknownClient()
    {
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);

        var response = await admin.PostAsJsonAsync(
            $"/api/clients/{ClientsTestSeeds.NoSuchClientId}/contacts", ClientsTestSeeds.ContactBody("Probe"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await ClientsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.CLIENT_NOT_FOUND);
    }

    /// <summary>Adding a contact is not for anonymous callers.</summary>
    [Fact]
    public async Task AddRefusesAnonymous()
    {
        var response = await factory.CreateClient().PostAsJsonAsync(
            $"/api/clients/{ClientsTestSeeds.NoSuchClientId}/contacts", ClientsTestSeeds.ContactBody("Probe"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client, who is not staff, is refused with 403 on the admin route.</summary>
    [Fact]
    public async Task AddRefusesAClient()
    {
        var (client, clientId, _) = await ClientsTestSeeds.RegisterClientAsync(factory, "add-contact-forbidden");

        var response = await client.PostAsJsonAsync($"/api/clients/{clientId}/contacts", ClientsTestSeeds.ContactBody("Probe"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// A contact added, updated and removed through the client portal's own
    /// <c>POST /api/clients/me/contacts</c> round-trips the same way the admin route does.
    /// </summary>
    /// <remarks>Split from the deleted <c>MyProfileEndpointTests</c> (A7 Task 5).</remarks>
    [Fact]
    public async Task MyContactsAddUpdateRemoveRoundTrips()
    {
        var (client, _, _) = await ClientsTestSeeds.RegisterClientAsync(factory, "my-contacts-happy");

        var added = await client.PostAsJsonAsync("/api/clients/me/contacts", ClientsTestSeeds.ContactBody("Billing"));
        added.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listed = await client.GetFromJsonAsync<JsonElement>("/api/clients/me/contacts");
        var contactId = listed.EnumerateArray().Single().GetProperty("id").GetInt32();

        var updated = await client.PutAsJsonAsync($"/api/clients/me/contacts/{contactId}", ClientsTestSeeds.ContactBody("Renamed"));
        updated.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var removed = await client.DeleteAsync($"/api/clients/me/contacts/{contactId}");
        removed.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterRemoval = await client.GetFromJsonAsync<JsonElement>("/api/clients/me/contacts");
        afterRemoval.GetArrayLength().Should().Be(0);
    }
}
