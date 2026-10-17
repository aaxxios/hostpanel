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

/// <summary><c>PUT /api/clients/me</c>: the authenticated client updates their own profile.</summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class UpdateMyProfileTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>The caller's own profile is written and answered back, with the currency left alone.</summary>
    [Fact]
    public async Task UpdateMyProfileWritesTheOwnProfileAndIgnoresCurrency()
    {
        var (client, _, _) = await ClientsTestSeeds.RegisterClientAsync(factory, "update-my-profile");
        var before = await client.GetFromJsonAsync<JsonElement>("/api/clients/me");

        var response = await client.PutAsJsonAsync(
            "/api/clients/me", ClientsTestSeeds.UpdateBody(firstName: "Renamed", currency: "JPY"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("firstName").GetString().Should().Be("Renamed");
        body.GetProperty("currency").GetString().Should().Be(before.GetProperty("currency").GetString());
    }

    /// <summary>A caller with the Client role but no client row answers 404, not a write.</summary>
    [Fact]
    public async Task UpdateMyProfileWithNoClientRowRefusesNotFound()
    {
        var (client, clientId, _) = await ClientsTestSeeds.RegisterClientAsync(factory, "update-my-profile-orphaned");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Clients.Where(c => c.Id == clientId).ExecuteDeleteAsync();
        }

        var response = await client.PutAsJsonAsync("/api/clients/me", ClientsTestSeeds.UpdateBody());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        await ClientsTestSeeds.AssertCodeAsync(response, HostpanelErrorCodes.CLIENT_PROFILE_NOT_FOUND);
    }

    /// <summary>The route is not for anonymous callers.</summary>
    [Fact]
    public async Task UpdateMyProfileRefusesAnonymous()
    {
        var response = await factory.CreateClient().PutAsJsonAsync("/api/clients/me", ClientsTestSeeds.UpdateBody());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>An admin, who is not a client, is refused with 403.</summary>
    [Fact]
    public async Task UpdateMyProfileRefusesAnAdmin()
    {
        var client = factory.CreateClient();
        var token = await factory.GetAdminTokenAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PutAsJsonAsync("/api/clients/me", ClientsTestSeeds.UpdateBody());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
