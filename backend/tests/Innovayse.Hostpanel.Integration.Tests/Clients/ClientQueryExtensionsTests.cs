using FluentAssertions;
using Innovayse.Hostpanel.Application.Clients.Common;
using Innovayse.Hostpanel.Application.Clients.Extensions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Clients;

/// <summary>
/// <see cref="ClientQueryExtensions"/>, the lookup of the client behind a caller's token and the
/// include list every "load the whole client" use case shares, run against the real PostgreSQL host.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ClientQueryExtensionsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Seeds a client with one contact and returns its identity subject and id.</summary>
    /// <returns>The subject the client is linked to, and the client id.</returns>
    private async Task<(string UserId, int ClientId)> SeedClientWithContactAsync()
    {
        var userId = $"user-{Guid.NewGuid():N}";
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var client = Client.Create(userId, "Ada", "Lovelace", $"{userId}@example.test");
        client.AddContact(
            "Grace", "Hopper", null, $"contact-{userId}@example.test", null, ContactType.Billing,
            null, null, null, null, null, null, true, true, true, true, true, true);
        context.Clients.Add(client);
        await context.SaveChangesAsync(CancellationToken.None);
        return (userId, client.Id);
    }

    /// <summary>The caller's client is found by subject and comes back untracked.</summary>
    [Fact]
    public async Task FindReadOnlyByUserIdAsync_FindsTheClientWithoutTrackingItAsync()
    {
        var (userId, clientId) = await SeedClientWithContactAsync();
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();

        var found = await context.Clients.FindReadOnlyByUserIdAsync(userId, CancellationToken.None);

        found.Should().NotBeNull();
        found!.Id.Should().Be(clientId);
        ((DbContext)context).Entry(found).State.Should().Be(EntityState.Detached);
    }

    /// <summary>A subject with no client record yields null, not an exception.</summary>
    [Fact]
    public async Task FindReadOnlyByUserIdAsync_ForAnUnknownSubject_ReturnsNullAsync()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();

        var found = await context.Clients.FindReadOnlyByUserIdAsync($"nobody-{Guid.NewGuid():N}", CancellationToken.None);

        found.Should().BeNull();
    }

    /// <summary>The shared include list brings the contacts along with the client.</summary>
    [Fact]
    public async Task IncludeChildren_LoadsTheContactsAsync()
    {
        var (_, clientId) = await SeedClientWithContactAsync();
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();

        var client = await context.Clients.IncludeChildren().FirstAsync(c => c.Id == clientId);

        client.Contacts.Should().ContainSingle().Which.FirstName.Should().Be("Grace");
    }
}
