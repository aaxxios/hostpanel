using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Queries.GetOwnedDomainNames;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Innovayse.Hostpanel.Integration.Tests.Domains;

/// <summary>
/// <see cref="GetOwnedDomainNamesQuery"/> through the real <see cref="IMessageBus"/>: which of the
/// named domains a client holds, whatever case the names arrive in.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class GetOwnedDomainNamesTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Only the client's own domains come back, matched without regard to letter case.</summary>
    [Fact]
    public async Task HandleAsync_MixedNames_ReturnsOnlyTheClientsOwnInLowerCaseAsync()
    {
        int clientId;
        var mine = $"mine-{Guid.NewGuid():N}"[..18] + ".example";
        var theirs = $"theirs-{Guid.NewGuid():N}"[..18] + ".example";
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
            clientId = await DomainsTestSeeds.SeedClientAsync(context);
            var otherClientId = await DomainsTestSeeds.SeedClientAsync(context);
            context.Domains.Add(RegisteredDomain.Register(
                clientId, mine, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1), autoRenew: false, whoisPrivacy: false, priceCurrency: TestCurrencies.Base));
            context.Domains.Add(RegisteredDomain.Register(
                otherClientId, theirs, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1), autoRenew: false, whoisPrivacy: false, priceCurrency: TestCurrencies.Base));
            await context.SaveChangesAsync(CancellationToken.None);
        }

        Result<IReadOnlyList<string>> result;
        using (var scope = factory.Services.CreateScope())
        {
            var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
            result = await bus.InvokeAsync<Result<IReadOnlyList<string>>>(
                new GetOwnedDomainNamesQuery(clientId, [mine.ToUpperInvariant(), theirs, "missing.example"]));
        }

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo([mine.ToLowerInvariant()]);
    }
}
