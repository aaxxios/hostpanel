using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Clients;

/// <summary><c>DELETE /api/clients/{id}/contacts/{contactId}</c>: removing a contact from a client account.</summary>
/// <remarks>Split from the deleted <c>ClientsAdminEndpointTests</c> (A7 Task 5).</remarks>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class RemoveContactTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Removing an existing contact takes it off the client's record.</summary>
    [Fact]
    public async Task RemoveDropsTheContact()
    {
        var (clientId, _) = await ClientsTestSeeds.SeedClientAsync(factory);
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);
        var added = await admin.PostAsJsonAsync($"/api/clients/{clientId}/contacts", ClientsTestSeeds.ContactBody("Billing"));
        added.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var contacts = await admin.GetFromJsonAsync<JsonElement>($"/api/clients/{clientId}");
        var contactId = contacts.GetProperty("contacts").EnumerateArray().Single().GetProperty("id").GetInt32();

        var removed = await admin.DeleteAsync($"/api/clients/{clientId}/contacts/{contactId}");

        removed.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var fetched = await admin.GetFromJsonAsync<JsonElement>($"/api/clients/{clientId}");
        fetched.GetProperty("contacts").GetArrayLength().Should().Be(0);
    }

    /// <summary>Removing a contact on an id that names no client answers 404 with <c>CLIENT_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task RemoveRefusesUnknownClient()
    {
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);

        var response = await admin.DeleteAsync($"/api/clients/{ClientsTestSeeds.NoSuchClientId}/contacts/1");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await ClientsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.CLIENT_NOT_FOUND);
    }

    /// <summary>Removing a contact id that is not this client's answers 404 with <c>MY_CONTACT_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task RemoveRefusesAWrongContactId()
    {
        var (clientId, _) = await ClientsTestSeeds.SeedClientAsync(factory);
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);
        const int wrongContactId = 999_999;

        var response = await admin.DeleteAsync($"/api/clients/{clientId}/contacts/{wrongContactId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await ClientsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.MY_CONTACT_NOT_FOUND);
    }

    /// <summary>Removing a contact is not for anonymous callers.</summary>
    [Fact]
    public async Task RemoveRefusesAnonymous()
    {
        var response = await factory.CreateClient().DeleteAsync($"/api/clients/{ClientsTestSeeds.NoSuchClientId}/contacts/1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client, who is not staff, is refused with 403 on the admin route.</summary>
    [Fact]
    public async Task RemoveRefusesAClient()
    {
        var (client, clientId, _) = await ClientsTestSeeds.RegisterClientAsync(factory, "remove-contact-forbidden");

        var response = await client.DeleteAsync($"/api/clients/{clientId}/contacts/1");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
