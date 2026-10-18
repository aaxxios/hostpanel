using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Innovayse.Hostpanel.Application.Common.Interfaces;
using Innovayse.Hostpanel.Application.Domains.Commands.SyncTldCostPrices;
using Innovayse.Hostpanel.Domain.Domains;
using Innovayse.Hostpanel.Domain.Domains.Enums;
using Innovayse.Hostpanel.Integration.Tests.Fixtures;
using Innovayse.Hostpanel.Shared.Models;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;

namespace Innovayse.Hostpanel.Integration.Tests.Domains;

/// <summary>
/// Integration tests for the <c>SyncTldCostPrices</c> use case, both as the scheduled job
/// dispatched through the real bus (<c>IMessageBus.InvokeAsync</c>) and as the admin-triggered
/// <c>POST /api/admin/tld-configs/sync</c> endpoint -- the same command, two callers. Converted
/// from <c>ITldConfigRepository.ListAllAsync</c> to an inlined <c>IHostpanelDbContext</c> query
/// in A6; this had zero Integration coverage before that batch.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host.</param>
public sealed class SyncTldCostPricesTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>
    /// Syncing cost prices groups every TLD configuration by its registrar module and updates
    /// the cost columns of the ones the (fake) provider's pricing list names -- the query
    /// <c>SyncTldCostPricesHandler</c> loads and groups by <see cref="TldConfig.RegistrarModule"/>.
    /// Uses <see cref="FakeRegistrarProviderFactory"/> so the sync never resolves a real,
    /// credentialed registrar client.
    /// </summary>
    [Fact]
    public async Task SyncTldCostPrices_UpdatesCostPricesForAMatchingTldAsync()
    {
        var pricingProvider = new FakeTldPricingProvider
        {
            PricingToReturn =
            [
                new TldPricing(
                    "synced",
                    "USD",
                    Register: new Dictionary<int, decimal> { [1] = 9m },
                    Transfer: new Dictionary<int, decimal> { [1] = 10m },
                    Renew: new Dictionary<int, decimal> { [1] = 11m }),
            ],
        };
        var registrars = new FakeRegistrarProviderFactory();
        registrars.Register(RegistrarModule.NameAm, pricingProvider);

        var host = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.AddScoped<IRegistrarProviderFactory>(_ => registrars)));

        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IHostpanelDbContext>();
        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var tld = TldConfig.Create(
            "synced", RegistrarModule.NameAm, "USD", "USD", isEnabled: true, sortOrder: 0, categories: []);
        context.TldConfigs.Add(tld);
        await context.SaveChangesAsync(CancellationToken.None);

        var outcome = await bus.InvokeAsync<Result>(new SyncTldCostPricesCommand(), CancellationToken.None);

        Assert.True(outcome.IsSuccess);
        Assert.Equal(1, pricingProvider.PricingCallCount);
        var reloaded = await context.TldConfigs.AsNoTracking().FirstAsync(t => t.Id == tld.Id, CancellationToken.None);
        Assert.Equal(9m, reloaded.CostRegister[1]);
    }

    /// <summary>Triggering a manual sync through the admin endpoint succeeds and reaches every configured registrar.</summary>
    [Fact]
    public async Task SyncSucceedsAndPollsEveryConfiguredRegistrarAsync()
    {
        var (_, body) = DomainsTestSeeds.NewTldBody();
        var provider = new FakeTldPricingProvider();
        var registrars = new FakeRegistrarProviderFactory();
        registrars.Register(RegistrarModule.NameAm, provider);

        var host = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.AddScoped<IRegistrarProviderFactory>(_ => registrars)));
        var token = await factory.GetAdminTokenAsync();
        var admin = host.CreateClient();
        admin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        (await admin.PostAsJsonAsync("/api/admin/tld-configs", body)).StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await admin.PostAsync("/api/admin/tld-configs/sync", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        provider.PricingCallCount.Should().Be(1);
    }

    /// <summary>An unauthenticated caller is refused with 401 on the admin-triggered sync.</summary>
    [Fact]
    public async Task Sync_WithoutToken_Returns401Async()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsync("/api/admin/tld-configs/sync", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
