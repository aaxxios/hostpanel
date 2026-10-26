using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Errors;
using Innovayse.Hostpanel.Infrastructure.Persistence;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Clients;

/// <summary><c>POST /api/clients/me/contacts</c>: the authenticated client adds a contact to their own account.</summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class AddMyContactTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>The contact lands on the caller's own account.</summary>
    [Fact]
    public async Task AddMyContactWritesToTheOwnAccount()
    {
        var (client, _, _) = await ClientsTestSeeds.RegisterClientAsync(factory, "add-my-contact");

        var response = await client.PostAsJsonAsync("/api/clients/me/contacts", ClientsTestSeeds.ContactBody("Billing"));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var listed = await client.GetFromJsonAsync<JsonElement>("/api/clients/me/contacts");
        listed.EnumerateArray().Single().GetProperty("firstName").GetString().Should().Be("Billing");
    }

    /// <summary>A caller with the Client role but no client row owns no account and is linked to none, so the permission gate refuses before any write with 403 <c>CLIENT_PERMISSION_DENIED</c>.</summary>
    [Fact]
    public async Task AddMyContactWithNoClientRowIsRefusedByThePermissionGate()
    {
        var (client, clientId, _) = await ClientsTestSeeds.RegisterClientAsync(factory, "add-my-contact-orphaned");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Clients.Where(c => c.Id == clientId).ExecuteDeleteAsync();
        }

        var response = await client.PostAsJsonAsync("/api/clients/me/contacts", ClientsTestSeeds.ContactBody("Billing"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        await ClientsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.CLIENT_PERMISSION_DENIED);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task AddMyContactRefusesAnonymous()
    {
        var response = await factory.CreateClient()
            .PostAsJsonAsync("/api/clients/me/contacts", ClientsTestSeeds.ContactBody("Billing"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>An admin, who is not a client, is refused with 403.</summary>
    [Fact]
    public async Task AddMyContactRefusesAnAdmin()
    {
        var client = factory.CreateClient();
        var token = await factory.GetAdminTokenAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/clients/me/contacts", ClientsTestSeeds.ContactBody("Billing"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
