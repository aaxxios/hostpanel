using System.Net;
using System.Text.Json;
using Innovayse.Hostpanel.Application.Common.Errors;

namespace Innovayse.Hostpanel.Integration.Tests.Clients;

/// <summary><c>GET /api/clients/{id}</c>: an admin reads a single client's full detail.</summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetClientTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Reading an existing client answers 200 with its profile.</summary>
    [Fact]
    public async Task GetExistingClientReturnsTheProfile()
    {
        var (clientId, _) = await ClientsTestSeeds.SeedClientAsync(factory, firstName: "Ada", lastName: "Lovelace");
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);

        var response = await admin.GetAsync($"/api/clients/{clientId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("firstName").GetString().Should().Be("Ada");
    }

    /// <summary>Reading an id that names no client answers 404 with <c>CLIENT_NOT_FOUND</c>.</summary>
    [Fact]
    public async Task GetUnknownClientRefusesNotFound()
    {
        var admin = await ClientsTestSeeds.AdminClientAsync(factory);

        var response = await admin.GetAsync($"/api/clients/{ClientsTestSeeds.NoSuchClientId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await ClientsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.CLIENT_NOT_FOUND);
    }

    /// <summary>Reading a client is not for anonymous callers.</summary>
    [Fact]
    public async Task GetRefusesAnonymous()
    {
        var response = await factory.CreateClient().GetAsync($"/api/clients/{ClientsTestSeeds.NoSuchClientId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>A client, who is not staff, is refused with 403.</summary>
    [Fact]
    public async Task GetRefusesAClient()
    {
        var (client, clientId, _) = await ClientsTestSeeds.RegisterClientAsync(factory, "get-forbidden");

        var response = await client.GetAsync($"/api/clients/{clientId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
