using FluentAssertions;
using Innovayse.Hostpanel.Application.Common;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Notifications.Common;
using Innovayse.Hostpanel.Application.Notifications.Extensions;
using Innovayse.Hostpanel.Domain.Clients;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Notifications;

/// <summary>
/// <see cref="ClientRecipientExtensions"/>, the client-to-sign-in-address hop every client-facing
/// notification event handler starts from, run against the real PostgreSQL host and a fake identity port.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class ClientRecipientExtensionsTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Seeds a client and returns its id.</summary>
    /// <returns>The new client's id.</returns>
    private async Task<int> SeedClientAsync()
    {
        var userId = $"user-{Guid.NewGuid():N}";
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var client = Client.Create(userId, "Ada", "Lovelace", $"{userId}@example.test");
        context.Clients.Add(client);
        await context.SaveChangesAsync(CancellationToken.None);
        return client.Id;
    }

    /// <summary>A known client whose account the identity provider knows resolves to that account's address.</summary>
    [Fact]
    public async Task FindClientEmailAsync_ResolvesTheAccountAddressAsync()
    {
        var clientId = await SeedClientAsync();
        var identity = new FakeIdentityProvider(new IdentityAccount("subject", "ada@example.test", "Ada", "Lovelace"));
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();

        var email = await context.FindClientEmailAsync(identity, clientId, CancellationToken.None);

        email.Should().Be("ada@example.test");
    }

    /// <summary>A client id that does not exist has nobody to notify, and the identity provider is not asked.</summary>
    [Fact]
    public async Task FindClientEmailAsync_ForAnUnknownClient_ReturnsNullWithoutAskingIdentityAsync()
    {
        var identity = new FakeIdentityProvider(new IdentityAccount("subject", "ada@example.test", "Ada", "Lovelace"));
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();

        var email = await context.FindClientEmailAsync(identity, 999_999_999, CancellationToken.None);

        email.Should().BeNull();
        identity.SubjectLookups.Should().Be(0);
    }

    /// <summary>A client whose account the identity provider does not know has nobody to notify.</summary>
    [Fact]
    public async Task FindClientEmailAsync_WhenIdentityDoesNotKnowTheAccount_ReturnsNullAsync()
    {
        var clientId = await SeedClientAsync();
        var identity = new FakeIdentityProvider(null);
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();

        var email = await context.FindClientEmailAsync(identity, clientId, CancellationToken.None);

        email.Should().BeNull();
    }
}
