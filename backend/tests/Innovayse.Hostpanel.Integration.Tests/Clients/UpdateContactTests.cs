using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Clients;

/// <summary><c>PUT /api/clients/{id}/contacts/{contactId}</c>: editing an existing contact.</summary>
/// <remarks>Split from the deleted <c>ClientsAdminEndpointTests</c> (A7 Task 5).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class UpdateContactTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Updating a contact writes the change back.</summary>
    [Fact]
    public async Task UpdateWritesTheContact()
    {
        var (clientId, _) = await ClientsTestSeeds.SeedClientAsync(factory);
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);
        var added = await admin.PostAsJsonAsync($"/api/clients/{clientId}/contacts", ClientsTestSeeds.ContactBody("Billing"));
        added.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var contacts = await admin.GetFromJsonAsync<JsonElement>($"/api/clients/{clientId}");
        var contactId = contacts.GetProperty("contacts").EnumerateArray().Single().GetProperty("id").GetInt32();

        var response = await admin.PutAsJsonAsync(
            $"/api/clients/{clientId}/contacts/{contactId}", ClientsTestSeeds.ContactBody("Renamed"));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var fetched = await admin.GetFromJsonAsync<JsonElement>($"/api/clients/{clientId}");
        fetched.GetProperty("contacts").EnumerateArray().Single().GetProperty("firstName").GetString().Should().Be("Renamed");
    }

    /// <summary>Updating a contact on an id that names no client answers 404 with <c>CLIENT_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task UpdateRefusesUnknownClient()
    {
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);

        var response = await admin.PutAsJsonAsync(
            $"/api/clients/{ClientsTestSeeds.NoSuchClientId}/contacts/1", ClientsTestSeeds.ContactBody("Probe"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await ClientsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.CLIENT_NOT_FOUND);
    }

    /// <summary>Updating a contact id that is not this client's answers 404 with <c>MY_CONTACT_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task UpdateRefusesAWrongContactId()
    {
        var (clientId, _) = await ClientsTestSeeds.SeedClientAsync(factory);
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);
        const int wrongContactId = 999_999;

        var response = await admin.PutAsJsonAsync(
            $"/api/clients/{clientId}/contacts/{wrongContactId}", ClientsTestSeeds.ContactBody("Probe"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await ClientsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.MY_CONTACT_NOT_FOUND);
    }

    /// <summary>Updating a contact is not for anonymous callers.</summary>
    [Fact]
    public async Task UpdateRefusesAnonymous()
    {
        var response = await factory.CreateClient().PutAsJsonAsync(
            $"/api/clients/{ClientsTestSeeds.NoSuchClientId}/contacts/1", ClientsTestSeeds.ContactBody("Probe"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client, who is not staff, is refused with 403 on the admin route.</summary>
    [Fact]
    public async Task UpdateRefusesAClient()
    {
        var (client, clientId, _) = await ClientsTestSeeds.RegisterClientAsync(factory, "update-contact-forbidden");

        var response = await client.PutAsJsonAsync($"/api/clients/{clientId}/contacts/1", ClientsTestSeeds.ContactBody("Probe"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Updating a contact id that is not the caller's own, through the portal's own
    /// <c>PUT /api/clients/me/contacts/{contactId}</c>, answers 404 with <c>MY_CONTACT_NOT_FOUND</c>.
    /// </summary>
    /// <remarks>Split from the deleted <c>MyProfileEndpointTests</c> (A7 Task 5).</remarks>
    [Fact]
    public async Task MyContactsWrongIdRefusesNotFound()
    {
        var (client, _, _) = await ClientsTestSeeds.RegisterClientAsync(factory, "my-update-contact-wrong-id");

        var response = await client.PutAsJsonAsync("/api/clients/me/contacts/999999", ClientsTestSeeds.ContactBody("Anyone"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await ClientsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.MY_CONTACT_NOT_FOUND);
    }
}
